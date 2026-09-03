using Mooch.Api.Entities.Dogs;

namespace Mooch.Api.Entities.Activities;

public sealed class ActivityDog
{
    public Guid ActivityDogID { get; set; } = Guid.NewGuid();
    public Guid ActivityID { get; set; }
    public Guid DogID { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public Activity Activity { get; set; } = null!;
    public Dog Dog { get; set; } = null!;
}
