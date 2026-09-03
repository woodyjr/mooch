using Mooch.Api.Features.Integrations.Strava.Contracts;
using Mooch.Api.Infrastructure.Http;
using System.Security.Claims;

namespace Mooch.Api.Features.Integrations.Strava;

public interface IStravaService
{
    string CreateAuthorizationUrl(string state);
    Task<Result<StravaStatusResponse>> GetStatusAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ImportedActivityResponse>>> GetImportedActivitiesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<StravaSyncResponse>> SyncAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<Result<bool>> CompleteConnectionAsync(ClaimsPrincipal principal, string code, CancellationToken cancellationToken);
}
