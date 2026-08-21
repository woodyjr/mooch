namespace Mooch.Api.Features.Dogs.Contracts.Profile;

public sealed class DashboardResponse
{
    public Guid DogID { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal WeeklyGoalMiles { get; init; }
    public decimal WeeklyMiles { get; init; }
    public int WeeklyAdventures { get; init; }
    public int WeeklyOutsideMinutes { get; init; }
    public int StreakDays { get; init; }
    public int PackRank { get; init; }
    public int OwnerCount { get; init; }
    public IReadOnlyList<ActivitySummaryResponse> RecentActivities { get; init; } = [];
    public WeeklyChallengeResponse WeeklyChallenge { get; init; } = new();
}

public sealed class ActivitySummaryResponse
{
    public Guid ActivityID { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal DistanceMiles { get; init; }
    public int DurationMinutes { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
}

public sealed class WeeklyChallengeResponse
{
    public string Name { get; init; } = string.Empty;
    public decimal GoalMiles { get; init; }
    public decimal RemainingMiles { get; init; }
}

