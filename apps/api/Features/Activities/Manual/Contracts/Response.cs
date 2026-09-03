namespace Mooch.Api.Features.Activities.Manual.Contracts;

public sealed class Response
{
    public Guid ActivityID { get; init; }
    public IReadOnlyList<Guid> DogIDs { get; init; } = [];
    public Guid WalkerID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public decimal DistanceMiles { get; init; }
    public int DurationMinutes { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public string Source { get; init; } = "manual";
}
