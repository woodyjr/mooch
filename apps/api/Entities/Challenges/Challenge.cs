namespace Mooch.Api.Entities.Challenges;

public sealed class Challenge
{
    public Guid ChallengeID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }

    public ICollection<ChallengeParticipant> Participants { get; set; } = [];
}
