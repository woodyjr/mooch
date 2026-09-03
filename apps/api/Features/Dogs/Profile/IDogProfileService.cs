using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Dogs.Contracts.Profile;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Mooch.Api.Features.Dogs.Profile;

public interface IDogProfileService
{
    Task<Result<IReadOnlyList<Response>>> GetDogsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<Response>> CreateDogAsync(ClaimsPrincipal principal, CreateRequest request, CancellationToken cancellationToken);
    Task<Result<Response>> UpdateDogAsync(ClaimsPrincipal principal, Guid dogId, UpdateRequest request, CancellationToken cancellationToken);
    Task<Result<Response>> UploadAvatarAsync(ClaimsPrincipal principal, Guid dogId, IFormFile? file, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteDogAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken);
    Task<Result<DashboardResponse>> GetDashboardAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken);
}

