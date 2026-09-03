using Mooch.Api.Entities.Integrations;
using Mooch.Api.Entities.Walkers;

namespace Mooch.Api.Entities.Activities;

public sealed class Activity
{
    public Guid ActivityID { get; set; } = Guid.NewGuid();
    public Guid WalkerID { get; set; }
    public Guid? ConnectedAccountID { get; set; }
    public string? ExternalActivityID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ActivitySourceType Source { get; set; } = ActivitySourceType.Manual;
    public decimal DistanceMiles { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ActivityDog> ActivityDogs { get; set; } = [];
    public ConnectedAccount? ConnectedAccount { get; set; }
    public Walker Walker { get; set; } = null!;
}
