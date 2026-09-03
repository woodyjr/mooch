namespace Mooch.Api.Features.Activities.Manual.Contracts;

public sealed class CreateRequest
{
    public Guid[] DogIDs { get; init; } = [];
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public decimal DistanceMiles { get; init; }
    public int DurationMinutes { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
}
