using Mooch.Api.Data;
using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Dogs;
using Mooch.Api.Entities.Walkers;
using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Auth;
using Mooch.Api.Features.Dogs.Contracts.Ownership;
using Mooch.Api.Features.Dogs.Contracts.Profile;
using Mooch.Api.Features.Dogs.Ownership;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mooch.Api.Features.Dogs.Profile;

public sealed class DogProfileService : IDogProfileService
{
    private const decimal WeeklyGoalMiles = 10m;

    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly DevelopmentDogSeeder _developmentDogSeeder;
    private readonly IDogOwnershipService _dogOwnershipService;

    public DogProfileService(
        AppDbContext dbContext,
        IAuthService authService,
        DevelopmentDogSeeder developmentDogSeeder,
        IDogOwnershipService dogOwnershipService)
    {
        _dbContext = dbContext;
        _authService = authService;
        _developmentDogSeeder = developmentDogSeeder;
        _dogOwnershipService = dogOwnershipService;
    }

    public async Task<Result<IReadOnlyList<Response>>> GetDogsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<IReadOnlyList<Response>>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var walker = await _authService.EnsureWalkerAsync(user, cancellationToken);
        await _developmentDogSeeder.EnsureSeededAsync(walker, cancellationToken);

        var dogs = await _dbContext.Dogs
            .AsNoTracking()
            .Where(x => x.WalkerDogs.Any(y => y.Walker.UserID == user.Id))
            .OrderBy(x => x.Name)
            .Select(x => new Response
            {
                Id = x.DogID,
                Name = x.Name,
                Breed = x.Breed,
                BirthDate = x.BirthDate,
                WeightPounds = x.WeightPounds,
                Bio = x.Bio,
                AvatarImage = x.AvatarImage,
                CreatedDateUtc = x.CreatedDateUtc,
                OwnerCount = x.WalkerDogs.Count,
                IsPrimaryOwner = x.WalkerDogs.Any(y => y.Walker.UserID == user.Id && y.IsPrimaryOwner)
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Response>>.Success(dogs);
    }

    public async Task<Result<Response>> CreateDogAsync(ClaimsPrincipal principal, CreateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Response>.Validation(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = ["Name is required."]
            });
        }

        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<Response>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var walker = await _authService.EnsureWalkerAsync(user, cancellationToken);
        var dog = new Dog
        {
            Name = request.Name.Trim(),
            Breed = request.Breed?.Trim(),
            BirthDate = request.BirthDate,
            WeightPounds = request.WeightPounds,
            Bio = request.Bio?.Trim(),
            AvatarImage = request.AvatarImage?.Trim()
        };

        _dbContext.Dogs.Add(dog);
        _dbContext.WalkerDogs.Add(new WalkerDog
        {
            WalkerID = walker.WalkerID,
            DogID = dog.DogID,
            IsPrimaryOwner = true
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.CoOwnerEmail))
        {
            var inviteResult = await _dogOwnershipService.InviteOwnerAsync(
                principal,
                dog.DogID,
                new InviteOwnerRequest { Email = request.CoOwnerEmail },
                cancellationToken);

            if (inviteResult.Error is not null)
            {
                return Result<Response>.Failure(inviteResult.StatusCode, inviteResult.Error);
            }
        }

        return Result<Response>.Success(dog.ToResponse(ownerCount: 1, isPrimaryOwner: true), StatusCodes.Status201Created);
    }

    public async Task<Result<DashboardResponse>> GetDashboardAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<DashboardResponse>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var walker = await _authService.EnsureWalkerAsync(user, cancellationToken);
        await _developmentDogSeeder.EnsureSeededAsync(walker, cancellationToken);

        var dog = await _dbContext.Dogs
            .AsNoTracking()
            .Where(x => x.DogID == dogId && x.WalkerDogs.Any(y => y.Walker.UserID == user.Id))
            .Select(x => new
            {
                x.DogID,
                x.Name,
                OwnerCount = x.WalkerDogs.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dog is null)
        {
            return Result<DashboardResponse>.Failure(StatusCodes.Status404NotFound, "That dog could not be found.");
        }

        var now = DateTimeOffset.UtcNow;
        var weekStart = GetStartOfWeekUtc(now);
        var allActivities = await _dbContext.Activities
            .AsNoTracking()
            .Where(x => x.DogID == dogId)
            .OrderByDescending(x => x.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var weeklyActivities = allActivities
            .Where(x => x.StartedAtUtc >= weekStart)
            .ToList();

        var weeklyMiles = weeklyActivities.Sum(x => x.DistanceMiles);
        var weeklyAdventures = weeklyActivities.Count;
        var weeklyOutsideMinutes = weeklyActivities.Sum(x => x.DurationMinutes);
        var streakDays = CalculateStreakDays(allActivities, now);

        var ranking = await _dbContext.Activities
            .AsNoTracking()
            .Where(x => x.StartedAtUtc >= weekStart)
            .GroupBy(x => x.DogID)
            .Select(x => new
            {
                DogID = x.Key,
                Miles = x.Sum(y => y.DistanceMiles)
            })
            .OrderByDescending(x => x.Miles)
            .ThenBy(x => x.DogID)
            .ToListAsync(cancellationToken);

        var packRank = ranking.FindIndex(x => x.DogID == dogId) + 1;
        if (packRank <= 0)
        {
            packRank = ranking.Count + 1;
        }

        var response = new DashboardResponse
        {
            DogID = dog.DogID,
            Name = dog.Name,
            WeeklyGoalMiles = WeeklyGoalMiles,
            WeeklyMiles = weeklyMiles,
            WeeklyAdventures = weeklyAdventures,
            WeeklyOutsideMinutes = weeklyOutsideMinutes,
            StreakDays = streakDays,
            PackRank = packRank,
            OwnerCount = dog.OwnerCount,
            RecentActivities = allActivities
                .Take(3)
                .Select(x => new ActivitySummaryResponse
                {
                    ActivityID = x.ActivityID,
                    Title = x.Title,
                    DistanceMiles = x.DistanceMiles,
                    DurationMinutes = x.DurationMinutes,
                    StartedAtUtc = x.StartedAtUtc
                })
                .ToList(),
            WeeklyChallenge = new WeeklyChallengeResponse
            {
                Name = "Cincinnati 10 Mile Club",
                GoalMiles = WeeklyGoalMiles,
                RemainingMiles = Math.Max(0, WeeklyGoalMiles - weeklyMiles)
            }
        };

        return Result<DashboardResponse>.Success(response);
    }

    private static DateTimeOffset GetStartOfWeekUtc(DateTimeOffset value)
    {
        var diff = ((int)value.DayOfWeek + 6) % 7;
        return value.Date.AddDays(-diff);
    }

    private static int CalculateStreakDays(IReadOnlyList<Activity> activities, DateTimeOffset now)
    {
        var activityDates = activities
            .Select(x => x.StartedAtUtc.UtcDateTime.Date)
            .Distinct()
            .ToHashSet();

        var currentDate = now.UtcDateTime.Date;
        var streak = 0;

        while (activityDates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }
}

