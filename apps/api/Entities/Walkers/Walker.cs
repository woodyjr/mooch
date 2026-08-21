using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Dogs;
using Mooch.Api.Entities.Users;

namespace Mooch.Api.Entities.Walkers;

public sealed class Walker
{
    public Guid WalkerID { get; set; } = Guid.NewGuid();
    public Guid UserID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarImage { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Activity> Activities { get; set; } = [];
    public ICollection<DogOwnerInvite> SentDogOwnerInvites { get; set; } = [];
    public AppUser User { get; set; } = null!;
    public ICollection<WalkerDog> WalkerDogs { get; set; } = [];
}
