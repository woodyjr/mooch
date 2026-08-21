using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Dogs.Contracts.Profile;
using System.Security.Claims;

namespace Mooch.Api.Features.Dogs.Profile;

public interface IDogProfileService
{
    Task<Result<IReadOnlyList<Response>>> GetDogsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<Response>> CreateDogAsync(ClaimsPrincipal principal, CreateRequest request, CancellationToken cancellationToken);
    Task<Result<DashboardResponse>> GetDashboardAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken);
}

