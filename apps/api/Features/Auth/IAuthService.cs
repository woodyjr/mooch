using Mooch.Api.Entities.Users;
using Mooch.Api.Entities.Walkers;
using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Auth.Contracts;
using System.Security.Claims;

namespace Mooch.Api.Features.Auth;

public interface IAuthService
{
    Task<Result<Response>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<Response>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<Response>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<Result<Response>> SocialLoginAsync(SocialLoginRequest request, CancellationToken cancellationToken);
    Task<Result<Response>> SocialRegisterAsync(SocialLoginRequest request, CancellationToken cancellationToken);
    Task LogoutAsync();
    Task<AppUser?> GetUserAsync(ClaimsPrincipal principal);
    Task<Walker> EnsureWalkerAsync(AppUser user, CancellationToken cancellationToken);
}

