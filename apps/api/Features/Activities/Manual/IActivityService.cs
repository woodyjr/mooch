using System.Security.Claims;
using Mooch.Api.Features.Activities.Manual.Contracts;
using Mooch.Api.Infrastructure.Http;

namespace Mooch.Api.Features.Activities.Manual;

public interface IActivityService
{
    Task<Result<Response>> CreateManualAsync(ClaimsPrincipal principal, CreateRequest request, CancellationToken cancellationToken);
}
