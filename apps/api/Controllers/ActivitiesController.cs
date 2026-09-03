using Mooch.Api.Features.Activities.Manual;
using Mooch.Api.Features.Activities.Manual.Contracts;
using Mooch.Api.Infrastructure.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mooch.Api.Controllers;

[ApiController]
[Route("api/activities")]
[Authorize]
public sealed class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpPost("manual")]
    public async Task<ActionResult<Response>> CreateManual(CreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _activityService.CreateManualAsync(User, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
