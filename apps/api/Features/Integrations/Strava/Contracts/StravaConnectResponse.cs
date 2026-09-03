namespace Mooch.Api.Features.Integrations.Strava.Contracts;

public sealed class StravaConnectResponse
{
    public string AuthorizationUrl { get; init; } = string.Empty;
}
