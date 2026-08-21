using Mooch.Api.Entities.Users;

namespace Mooch.Api.Entities.Challenges;

public sealed class ChallengeParticipant
{
    public Guid ChallengeParticipantID { get; set; } = Guid.NewGuid();
    public Guid ChallengeID { get; set; }
    public Guid UserID { get; set; }
    public DateTimeOffset JoinedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Challenge Challenge { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
