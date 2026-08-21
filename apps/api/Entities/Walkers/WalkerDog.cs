using Mooch.Api.Entities.Dogs;

namespace Mooch.Api.Entities.Walkers;

public sealed class WalkerDog
{
    public Guid WalkerDogID { get; set; } = Guid.NewGuid();
    public Guid WalkerID { get; set; }
    public Guid DogID { get; set; }
    public bool IsPrimaryOwner { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public Walker Walker { get; set; } = null!;
    public Dog Dog { get; set; } = null!;
}

