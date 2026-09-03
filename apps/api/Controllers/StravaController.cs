using Mooch.Api.Features.Integrations.Strava;
using Mooch.Api.Features.Integrations.Strava.Contracts;
using Mooch.Api.Infrastructure.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mooch.Api.Controllers;

[ApiController]
[Route("api/integrations/strava")]
[Authorize]
public sealed class StravaController : ControllerBase
{
    private const string StateCookieName = "mooch.strava.oauth_state";

    private readonly IStravaService _stravaService;
    private readonly StravaAuthOptions _options;

    public StravaController(IStravaService stravaService, IOptions<StravaAuthOptions> options)
    {
        _stravaService = stravaService;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<ActionResult<StravaStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var result = await _stravaService.GetStatusAsync(User, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("imports")]
    public async Task<ActionResult<IReadOnlyList<ImportedActivityResponse>>> GetImports(CancellationToken cancellationToken)
    {
        var result = await _stravaService.GetImportedActivitiesAsync(User, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("connect")]
    public ActionResult<StravaConnectResponse> BeginConnection()
    {
        var state = Guid.NewGuid().ToString("N");

        Response.Cookies.Append(StateCookieName, state, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10),
            Path = "/api/integrations/strava/callback"
        });

        return Ok(new StravaConnectResponse
        {
            AuthorizationUrl = _stravaService.CreateAuthorizationUrl(state)
        });
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error, CancellationToken cancellationToken)
    {
        var expectedState = Request.Cookies[StateCookieName];
        Response.Cookies.Delete(StateCookieName, new CookieOptions { Path = "/api/integrations/strava/callback" });

        if (!string.IsNullOrWhiteSpace(error))
        {
            return Redirect(BuildFrontendRedirect("denied"));
        }

        if (string.IsNullOrWhiteSpace(expectedState) || !string.Equals(expectedState, state, StringComparison.Ordinal))
        {
            return Redirect(BuildFrontendRedirect("state-mismatch"));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Redirect(BuildFrontendRedirect("missing-code"));
        }

        var result = await _stravaService.CompleteConnectionAsync(User, code, cancellationToken);
        if (result.Value != true)
        {
            return Redirect(BuildFrontendRedirect("connect-error"));
        }

        return Redirect(BuildFrontendRedirect("connected"));
    }

    [HttpPost("sync")]
    public async Task<ActionResult<StravaSyncResponse>> Sync(CancellationToken cancellationToken)
    {
        var result = await _stravaService.SyncAsync(User, cancellationToken);
        return this.ToActionResult(result);
    }

    private string BuildFrontendRedirect(string status)
    {
        var separator = _options.FrontendRedirectUri.Contains('?') ? '&' : '?';
        return $"{_options.FrontendRedirectUri}{separator}strava={Uri.EscapeDataString(status)}";
    }
}
