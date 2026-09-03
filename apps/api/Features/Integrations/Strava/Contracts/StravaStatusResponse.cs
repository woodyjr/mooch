namespace Mooch.Api.Features.Integrations.Strava.Contracts;

public sealed class StravaStatusResponse
{
    public bool IsConnected { get; init; }
    public int PendingImportCount { get; init; }
    public DateTimeOffset? LastImportedActivityAtUtc { get; init; }
}
