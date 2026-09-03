using Mooch.Api.Entities.Integrations;
using Mooch.Api.Entities.Walkers;

namespace Mooch.Api.Entities.Activities;

public sealed class ImportedActivity
{
    public Guid ImportedActivityID { get; set; } = Guid.NewGuid();
    public Guid WalkerID { get; set; }
    public Guid ConnectedAccountID { get; set; }
    public string ExternalActivityID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public ActivitySourceType Source { get; set; } = ActivitySourceType.Strava;
    public decimal DistanceMiles { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public bool RequiresDogAssignment { get; set; } = true;
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public ConnectedAccount ConnectedAccount { get; set; } = null!;
    public Walker Walker { get; set; } = null!;
}
