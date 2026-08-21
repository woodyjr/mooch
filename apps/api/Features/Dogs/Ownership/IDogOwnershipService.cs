using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Dogs.Contracts.Ownership;
using System.Security.Claims;

namespace Mooch.Api.Features.Dogs.Ownership;

public interface IDogOwnershipService
{
    Task<Result<IReadOnlyList<OwnerInviteResponse>>> GetOwnerInvitesAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken);
    Task<Result<OwnerInviteResponse>> InviteOwnerAsync(ClaimsPrincipal principal, Guid dogId, InviteOwnerRequest request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<PendingOwnerInviteResponse>>> GetPendingOwnerInvitesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<OwnerInviteResponse>> AcceptOwnerInviteAsync(ClaimsPrincipal principal, Guid inviteId, CancellationToken cancellationToken);
    Task<Result<OwnerInviteResponse>> DeclineOwnerInviteAsync(ClaimsPrincipal principal, Guid inviteId, CancellationToken cancellationToken);
}

