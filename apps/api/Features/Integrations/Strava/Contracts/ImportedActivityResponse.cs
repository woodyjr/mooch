namespace Mooch.Api.Features.Integrations.Strava.Contracts;

public sealed class ImportedActivityResponse
{
    public Guid ImportedActivityID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ActivityType { get; init; } = string.Empty;
    public decimal DistanceMiles { get; init; }
    public int DurationMinutes { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public bool RequiresDogAssignment { get; init; }
}
