using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Dogs;
using Mooch.Api.Entities.Walkers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Mooch.Api.Data;

public sealed class DevelopmentDogSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IHostEnvironment _environment;

    public DevelopmentDogSeeder(AppDbContext dbContext, IHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task EnsureSeededAsync(Walker walker, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        var hasDogs = await _dbContext.WalkerDogs
            .AsNoTracking()
            .AnyAsync(x => x.WalkerID == walker.WalkerID, cancellationToken);

        if (hasDogs)
        {
            return;
        }

        var hank = new Dog
        {
            Name = "Hank",
            Breed = "Golden Retriever",
            Bio = "Neighborhood morale captain.",
            BirthDate = new DateOnly(2020, 4, 9)
        };
        var mia = new Dog
        {
            Name = "Mia",
            Breed = "Labrador Mix",
            Bio = "Trail scout and puddle finder.",
            BirthDate = new DateOnly(2021, 7, 14)
        };
        var mooch = new Dog
        {
            Name = "Mooch",
            Breed = "Australian Shepherd",
            Bio = "Always ready for one more lap.",
            BirthDate = new DateOnly(2019, 10, 2)
        };

        _dbContext.Dogs.AddRange(hank, mia, mooch);
        _dbContext.WalkerDogs.AddRange(
            CreateWalkerDog(walker, hank, true),
            CreateWalkerDog(walker, mia, false),
            CreateWalkerDog(walker, mooch, false));

        var now = DateTimeOffset.UtcNow;
        var thisMorning = new DateTimeOffset(now.Year, now.Month, now.Day, 11, 0, 0, TimeSpan.Zero);

        _dbContext.Activities.AddRange(
            CreateActivity(walker, "Morning neighborhood loop", 1.8m, 34, thisMorning, hank),
            CreateActivity(walker, "Evening stroll", 2.3m, 46, thisMorning.AddDays(-1).AddHours(7), hank),
            CreateActivity(walker, "Ault Park trail walk", 2.0m, 41, thisMorning.AddDays(-2).AddHours(1), hank),
            CreateActivity(walker, "Sunrise sniff session", 1.4m, 28, thisMorning.AddDays(-3), hank),
            CreateActivity(walker, "Creekside cool-down", 1.2m, 25, thisMorning.AddDays(-4), hank),
            CreateActivity(walker, "Mia's hill repeats", 3.4m, 52, thisMorning.AddDays(-1), mia),
            CreateActivity(walker, "Mooch park loop", 2.9m, 49, thisMorning.AddDays(-2), mooch));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static WalkerDog CreateWalkerDog(Walker walker, Dog dog, bool isPrimaryOwner) =>
        new()
        {
            WalkerID = walker.WalkerID,
            DogID = dog.DogID,
            IsPrimaryOwner = isPrimaryOwner
        };

    private static Activity CreateActivity(
        Walker walker,
        string title,
        decimal distanceMiles,
        int durationMinutes,
        DateTimeOffset startedAtUtc,
        params Dog[] dogs) =>
        new()
        {
            WalkerID = walker.WalkerID,
            Title = title,
            DistanceMiles = distanceMiles,
            DurationMinutes = durationMinutes,
            Source = ActivitySourceType.Manual,
            StartedAtUtc = startedAtUtc,
            ActivityDogs = dogs
                .Select(dog => new ActivityDog
                {
                    DogID = dog.DogID
                })
                .ToList()
        };
}
