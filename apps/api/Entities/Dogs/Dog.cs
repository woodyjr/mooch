using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Walkers;

namespace Mooch.Api.Entities.Dogs;

public sealed class Dog
{
    public Guid DogID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? WeightPounds { get; set; }
    public string? Bio { get; set; }
    public string? AvatarImage { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Activity> Activities { get; set; } = [];
    public ICollection<DogOwnerInvite> OwnerInvites { get; set; } = [];
    public ICollection<WalkerDog> WalkerDogs { get; set; } = [];
}

