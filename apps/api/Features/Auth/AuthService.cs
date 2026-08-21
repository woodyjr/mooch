using Mooch.Api.Data;
using Mooch.Api.Entities.Integrations;
using Mooch.Api.Entities.Users;
using Mooch.Api.Entities.Walkers;
using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Auth.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mooch.Api.Features.Auth;

public sealed class AuthService : IAuthService
{
    private const string AuthProviderClaimType = "mooch:auth_provider";

    private readonly AppDbContext _dbContext;
    private readonly IGoogleTokenVerifier _googleTokenVerifier;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthService(
        AppDbContext dbContext,
        IGoogleTokenVerifier googleTokenVerifier,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _dbContext = dbContext;
        _googleTokenVerifier = googleTokenVerifier;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<Response>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(principal);

        if (user is null)
        {
            return Result<Response>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var provider = ExternalAuthProvider.Normalize(principal.FindFirstValue(AuthProviderClaimType));
        return Result<Response>.Success(ToResponse(user, provider));
    }

    public async Task<Result<Response>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRegistration(request);
        if (validationErrors.Count > 0)
        {
            return Result<Response>.Validation(validationErrors);
        }

        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return Result<Response>.Failure(StatusCodes.Status409Conflict, "An account with this email already exists.");
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            AvatarImage = request.AvatarImage
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Result<Response>.Validation(ToValidationErrors(createResult));
        }

        await EnsureWalkerAsync(user, cancellationToken);
        await SignInAsync(user, ExternalAuthProvider.Email);

        return Result<Response>.Success(
            ToResponse(user, ExternalAuthProvider.Email),
            StatusCodes.Status201Created);
    }

    public async Task<Result<Response>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = ValidateLogin(request);
        if (validationErrors.Count > 0)
        {
            return Result<Response>.Validation(validationErrors);
        }

        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<Response>.Failure(StatusCodes.Status401Unauthorized, "Invalid email or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            return Result<Response>.Failure(StatusCodes.Status401Unauthorized, "Invalid email or password.");
        }

        await EnsureWalkerAsync(user, cancellationToken);
        await SignInAsync(user, ExternalAuthProvider.Email);

        return Result<Response>.Success(ToResponse(user, ExternalAuthProvider.Email));
    }

    public async Task<Result<Response>> SocialLoginAsync(SocialLoginRequest request, CancellationToken cancellationToken)
    {
        var verification = await VerifySocialRequestAsync(request, cancellationToken);
        if (verification.ErrorResult is not null)
        {
            return verification.ErrorResult;
        }

        var provider = verification.Provider!;
        var identity = verification.Identity!;
        var connectedProvider = ExternalAuthProvider.ToConnectedAccountProvider(provider);
        var connectedAccount = await _dbContext.ConnectedAccounts
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Provider == connectedProvider && x.ExternalUserID == identity.Subject,
                cancellationToken);

        AppUser? user = connectedAccount?.User;

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(identity.Email);
            if (user is null)
            {
                return Result<Response>.Failure(
                    StatusCodes.Status404NotFound,
                    "No Mooch profile exists for this Google account yet. Create your account first.");
            }

            var linkResult = await LinkExternalAccountAsync(user, connectedProvider, identity.Subject, cancellationToken);
            if (linkResult is not null)
            {
                return linkResult;
            }

            var updateResult = await UpdateUserProfileFromIdentityAsync(user, identity);
            if (updateResult is not null)
            {
                return updateResult;
            }
        }

        await EnsureWalkerAsync(user, cancellationToken);
        await SignInAsync(user, provider);

        return Result<Response>.Success(ToResponse(user, provider));
    }

    public async Task<Result<Response>> SocialRegisterAsync(SocialLoginRequest request, CancellationToken cancellationToken)
    {
        var verification = await VerifySocialRequestAsync(request, cancellationToken);
        if (verification.ErrorResult is not null)
        {
            return verification.ErrorResult;
        }

        var provider = verification.Provider!;
        var identity = verification.Identity!;
        var connectedProvider = ExternalAuthProvider.ToConnectedAccountProvider(provider);

        var connectedAccount = await _dbContext.ConnectedAccounts
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Provider == connectedProvider && x.ExternalUserID == identity.Subject,
                cancellationToken);

        if (connectedAccount is not null)
        {
            return Result<Response>.Failure(
                StatusCodes.Status409Conflict,
                "A Mooch profile already exists for this Google account. Try signing in instead.");
        }

        var existingUser = await _userManager.FindByEmailAsync(identity.Email);
        if (existingUser is not null)
        {
            return Result<Response>.Failure(
                StatusCodes.Status409Conflict,
                "A Mooch profile already exists for this email. Sign in instead.");
        }

        var user = new AppUser
        {
            UserName = identity.Email,
            Email = identity.Email,
            EmailConfirmed = true,
            DisplayName = GetPreferredDisplayName(identity),
            AvatarImage = identity.Picture
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return Result<Response>.Validation(ToValidationErrors(createResult));
        }

        var linkResult = await LinkExternalAccountAsync(user, connectedProvider, identity.Subject, cancellationToken);
        if (linkResult is not null)
        {
            return linkResult;
        }

        await EnsureWalkerAsync(user, cancellationToken);
        await SignInAsync(user, provider);

        return Result<Response>.Success(ToResponse(user, provider), StatusCodes.Status201Created);
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();

    public Task<AppUser?> GetUserAsync(ClaimsPrincipal principal) => _userManager.GetUserAsync(principal);

    public async Task<Walker> EnsureWalkerAsync(AppUser user, CancellationToken cancellationToken)
    {
        var walker = await _dbContext.Walkers
            .FirstOrDefaultAsync(x => x.UserID == user.Id, cancellationToken);

        if (walker is not null)
        {
            return walker;
        }

        walker = new Walker
        {
            UserID = user.Id,
            DisplayName = user.DisplayName ?? user.UserName ?? user.Email ?? "Walker",
            AvatarImage = user.AvatarImage
        };

        _dbContext.Walkers.Add(walker);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return walker;
    }

    private async Task SignInAsync(AppUser user, string provider)
    {
        var claims = new[]
        {
            new Claim(AuthProviderClaimType, ExternalAuthProvider.Normalize(provider))
        };

        await _signInManager.SignInWithClaimsAsync(user, isPersistent: true, claims);
    }

    private async Task<(string? Provider, GoogleIdentity? Identity, Result<Response>? ErrorResult)> VerifySocialRequestAsync(
        SocialLoginRequest request,
        CancellationToken cancellationToken)
    {
        var provider = ExternalAuthProvider.Normalize(request.Provider);
        if (provider != ExternalAuthProvider.Google)
        {
            return (null, null, Result<Response>.Failure(StatusCodes.Status501NotImplemented, $"{provider} sign-in has not been wired yet."));
        }

        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            return (null, null, Result<Response>.Validation(new Dictionary<string, string[]>
            {
                [nameof(request.Credential)] = ["Google credential is required."]
            }));
        }

        try
        {
            var identity = await _googleTokenVerifier.VerifyAsync(request.Credential, cancellationToken);
            if (!identity.EmailVerified || string.IsNullOrWhiteSpace(identity.Email))
            {
                return (null, null, Result<Response>.Failure(StatusCodes.Status401Unauthorized, "Your Google account must have a verified email address."));
            }

            return (provider, identity, null);
        }
        catch (InvalidOperationException exception)
        {
            return (null, null, Result<Response>.Failure(StatusCodes.Status500InternalServerError, exception.Message));
        }
        catch (Exception)
        {
            return (null, null, Result<Response>.Failure(StatusCodes.Status401Unauthorized, "The Google sign-in token is invalid or expired."));
        }
    }

    private async Task<Result<Response>?> LinkExternalAccountAsync(
        AppUser user,
        ConnectedAccountProvider provider,
        string externalUserId,
        CancellationToken cancellationToken)
    {
        var existingProviderLink = await _dbContext.ConnectedAccounts
            .FirstOrDefaultAsync(
                x => x.UserID == user.Id && x.Provider == provider,
                cancellationToken);

        if (existingProviderLink is not null && existingProviderLink.ExternalUserID != externalUserId)
        {
            return Result<Response>.Failure(
                StatusCodes.Status409Conflict,
                "This Mooch profile is already linked to a different Google account.");
        }

        if (existingProviderLink is null)
        {
            _dbContext.ConnectedAccounts.Add(new ConnectedAccount
            {
                UserID = user.Id,
                Provider = provider,
                ExternalUserID = externalUserId,
                AccessToken = string.Empty
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return null;
    }

    private async Task<Result<Response>?> UpdateUserProfileFromIdentityAsync(AppUser user, GoogleIdentity identity)
    {
        var profileChanged = false;

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            profileChanged = true;
        }

        if (string.IsNullOrWhiteSpace(user.DisplayName) && !string.IsNullOrWhiteSpace(identity.Name))
        {
            user.DisplayName = identity.Name;
            profileChanged = true;
        }

        if (string.IsNullOrWhiteSpace(user.AvatarImage) && !string.IsNullOrWhiteSpace(identity.Picture))
        {
            user.AvatarImage = identity.Picture;
            profileChanged = true;
        }

        if (!profileChanged)
        {
            return null;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result<Response>.Validation(ToValidationErrors(updateResult));
        }

        return null;
    }

    private static Response ToResponse(AppUser user, string provider) =>
        new()
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? user.UserName ?? string.Empty,
            DisplayName = user.DisplayName ?? user.UserName ?? user.Email ?? "Walker",
            Provider = provider
        };

    private static string GetPreferredDisplayName(GoogleIdentity identity)
    {
        if (!string.IsNullOrWhiteSpace(identity.Name))
        {
            return identity.Name;
        }

        if (!string.IsNullOrWhiteSpace(identity.Email))
        {
            return identity.Email.Split('@', 2)[0];
        }

        return "Walker";
    }

    private static Dictionary<string, string[]> ValidateRegistration(RegisterRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors[nameof(request.Email)] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors[nameof(request.DisplayName)] = ["Display name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors[nameof(request.Password)] = ["Password is required."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateLogin(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors[nameof(request.Email)] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors[nameof(request.Password)] = ["Password is required."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ToValidationErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(x => x.Code)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Description).ToArray());
}

