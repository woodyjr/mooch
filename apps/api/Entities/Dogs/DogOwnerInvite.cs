using Mooch.Api.Entities.Walkers;

namespace Mooch.Api.Entities.Dogs;

public sealed class DogOwnerInvite
{
    public Guid DogOwnerInviteID { get; set; } = Guid.NewGuid();
    public Guid DogID { get; set; }
    public Guid InvitedByWalkerID { get; set; }
    public string InviteeEmail { get; set; } = string.Empty;
    public DogOwnerInviteStatus Status { get; set; } = DogOwnerInviteStatus.Pending;
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RespondedDateUtc { get; set; }

    public Dog Dog { get; set; } = null!;
    public Walker InvitedByWalker { get; set; } = null!;
}
