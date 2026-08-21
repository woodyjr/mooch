using Mooch.Api.Entities.Users;

namespace Mooch.Api.Entities.Friendships;

public sealed class Friendship
{
    public Guid FriendshipID { get; set; } = Guid.NewGuid();
    public Guid RequesterUserID { get; set; }
    public Guid AddresseeUserID { get; set; }
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public AppUser Addressee { get; set; } = null!;
    public AppUser Requester { get; set; } = null!;
}
