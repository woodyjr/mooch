namespace Mooch.Api.Features.Integrations.Strava.Contracts;

public sealed class StravaSyncResponse
{
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public int PendingImportCount { get; init; }
}
