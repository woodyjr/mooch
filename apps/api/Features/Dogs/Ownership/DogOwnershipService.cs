using Mooch.Api.Data;
using Mooch.Api.Entities.Dogs;
using Mooch.Api.Entities.Users;
using Mooch.Api.Entities.Walkers;
using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Auth;
using Mooch.Api.Features.Dogs.Contracts.Ownership;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mooch.Api.Features.Dogs.Ownership;

public sealed class DogOwnershipService : IDogOwnershipService
{
    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;

    public DogOwnershipService(AppDbContext dbContext, IAuthService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    public async Task<Result<IReadOnlyList<OwnerInviteResponse>>> GetOwnerInvitesAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<IReadOnlyList<OwnerInviteResponse>>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var authorized = await _dbContext.WalkerDogs
            .AsNoTracking()
            .AnyAsync(x => x.DogID == dogId && x.Walker.UserID == user.Id, cancellationToken);

        if (!authorized)
        {
            return Result<IReadOnlyList<OwnerInviteResponse>>.Failure(StatusCodes.Status404NotFound, "That dog could not be found.");
        }

        var invites = await _dbContext.DogOwnerInvites
            .AsNoTracking()
            .Where(x => x.DogID == dogId)
            .OrderByDescending(x => x.CreatedDateUtc)
            .Select(x => new OwnerInviteResponse
            {
                Id = x.DogOwnerInviteID,
                Email = x.InviteeEmail,
                Status = x.Status.ToString().ToLowerInvariant(),
                CreatedDateUtc = x.CreatedDateUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OwnerInviteResponse>>.Success(invites);
    }

    public async Task<Result<OwnerInviteResponse>> InviteOwnerAsync(ClaimsPrincipal principal, Guid dogId, InviteOwnerRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<OwnerInviteResponse>.Validation(new Dictionary<string, string[]>
            {
                [nameof(request.Email)] = ["Email is required."]
            });
        }

        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<OwnerInviteResponse>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var walker = await _authService.EnsureWalkerAsync(user, cancellationToken);
        var inviteResult = await InviteOwnerInternalAsync(user, walker, dogId, request.Email, cancellationToken);
        if (inviteResult.ErrorResult is not null)
        {
            return inviteResult.ErrorResult;
        }

        return Result<OwnerInviteResponse>.Success(inviteResult.Invite!, StatusCodes.Status201Created);
    }

    public async Task<Result<IReadOnlyList<PendingOwnerInviteResponse>>> GetPendingOwnerInvitesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<IReadOnlyList<PendingOwnerInviteResponse>>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var email = (user.Email ?? user.UserName ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<IReadOnlyList<PendingOwnerInviteResponse>>.Success([]);
        }

        var invites = await _dbContext.DogOwnerInvites
            .AsNoTracking()
            .Where(x => x.Status == DogOwnerInviteStatus.Pending && x.InviteeEmail == email)
            .OrderByDescending(x => x.CreatedDateUtc)
            .Select(x => new PendingOwnerInviteResponse
            {
                Id = x.DogOwnerInviteID,
                DogID = x.DogID,
                DogName = x.Dog.Name,
                InvitedByDisplayName = x.InvitedByWalker.DisplayName,
                CreatedDateUtc = x.CreatedDateUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PendingOwnerInviteResponse>>.Success(invites);
    }

    public Task<Result<OwnerInviteResponse>> AcceptOwnerInviteAsync(ClaimsPrincipal principal, Guid inviteId, CancellationToken cancellationToken) =>
        RespondToOwnerInviteAsync(principal, inviteId, DogOwnerInviteStatus.Accepted, cancellationToken);

    public Task<Result<OwnerInviteResponse>> DeclineOwnerInviteAsync(ClaimsPrincipal principal, Guid inviteId, CancellationToken cancellationToken) =>
        RespondToOwnerInviteAsync(principal, inviteId, DogOwnerInviteStatus.Declined, cancellationToken);

    private async Task<(OwnerInviteResponse? Invite, Result<OwnerInviteResponse>? ErrorResult)> InviteOwnerInternalAsync(
        AppUser user,
        Walker walker,
        Guid dogId,
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return (null, Result<OwnerInviteResponse>.Validation(new Dictionary<string, string[]>
            {
                [nameof(email)] = ["Email is required."]
            }));
        }

        var dog = await _dbContext.Dogs
            .Include(x => x.WalkerDogs)
                .ThenInclude(x => x.Walker)
                    .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.DogID == dogId, cancellationToken);

        if (dog is null || dog.WalkerDogs.All(x => x.Walker.UserID != user.Id))
        {
            return (null, Result<OwnerInviteResponse>.Failure(StatusCodes.Status404NotFound, "That dog could not be found."));
        }

        var currentUserEmail = (user.Email ?? user.UserName ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedEmail == currentUserEmail)
        {
            return (null, Result<OwnerInviteResponse>.Failure(StatusCodes.Status409Conflict, "You already own this dog profile."));
        }

        if (dog.WalkerDogs.Any(x => string.Equals(x.Walker.User.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
        {
            return (null, Result<OwnerInviteResponse>.Failure(StatusCodes.Status409Conflict, "That person already co-owns this dog profile."));
        }

        var existingPendingInvite = await _dbContext.DogOwnerInvites
            .FirstOrDefaultAsync(
                x => x.DogID == dogId &&
                     x.InviteeEmail.ToLower() == normalizedEmail &&
                     x.Status == DogOwnerInviteStatus.Pending,
                cancellationToken);

        if (existingPendingInvite is not null)
        {
            return (null, Result<OwnerInviteResponse>.Failure(StatusCodes.Status409Conflict, "An invite is already pending for that email."));
        }

        var invite = new DogOwnerInvite
        {
            DogID = dogId,
            InvitedByWalkerID = walker.WalkerID,
            InviteeEmail = normalizedEmail,
            Status = DogOwnerInviteStatus.Pending
        };

        _dbContext.DogOwnerInvites.Add(invite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (new OwnerInviteResponse
        {
            Id = invite.DogOwnerInviteID,
            Email = invite.InviteeEmail,
            Status = invite.Status.ToString().ToLowerInvariant(),
            CreatedDateUtc = invite.CreatedDateUtc
        }, null);
    }

    private async Task<Result<OwnerInviteResponse>> RespondToOwnerInviteAsync(
        ClaimsPrincipal principal,
        Guid inviteId,
        DogOwnerInviteStatus nextStatus,
        CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<OwnerInviteResponse>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var email = (user.Email ?? user.UserName ?? string.Empty).Trim().ToLowerInvariant();
        var invite = await _dbContext.DogOwnerInvites
            .Include(x => x.Dog)
            .FirstOrDefaultAsync(
                x => x.DogOwnerInviteID == inviteId &&
                     x.Status == DogOwnerInviteStatus.Pending &&
                     x.InviteeEmail == email,
                cancellationToken);

        if (invite is null)
        {
            return Result<OwnerInviteResponse>.Failure(StatusCodes.Status404NotFound, "Dog invite could not be found.");
        }

        var walker = await _authService.EnsureWalkerAsync(user, cancellationToken);

        if (nextStatus == DogOwnerInviteStatus.Accepted)
        {
            var alreadyLinked = await _dbContext.WalkerDogs
                .AnyAsync(x => x.DogID == invite.DogID && x.WalkerID == walker.WalkerID, cancellationToken);

            if (!alreadyLinked)
            {
                _dbContext.WalkerDogs.Add(new WalkerDog
                {
                    DogID = invite.DogID,
                    WalkerID = walker.WalkerID,
                    IsPrimaryOwner = false
                });
            }
        }

        invite.Status = nextStatus;
        invite.RespondedDateUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<OwnerInviteResponse>.Success(new OwnerInviteResponse
        {
            Id = invite.DogOwnerInviteID,
            Email = invite.InviteeEmail,
            Status = invite.Status.ToString().ToLowerInvariant(),
            CreatedDateUtc = invite.CreatedDateUtc
        });
    }
}

