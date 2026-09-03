using Mooch.Api.Data;
using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Dogs;
using Mooch.Api.Entities.Walkers;
using Mooch.Api.Features.Auth;
using Mooch.Api.Features.Dogs.Contracts.Ownership;
using Mooch.Api.Features.Dogs.Contracts.Profile;
using Mooch.Api.Features.Dogs.Ownership;
using Mooch.Api.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mooch.Api.Features.Dogs.Profile;

public sealed class DogProfileService : IDogProfileService
{
    private const decimal WeeklyGoalMiles = 10m;
    private const long MaxAvatarBytes = 5 * 1024 * 1024;

    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly DevelopmentDogSeeder _developmentDogSeeder;
    private readonly IDogOwnershipService _dogOwnershipService;
    private readonly IWebHostEnvironment _environment;

    public DogProfileService(
        AppDbContext dbContext,
        IAuthService authService,
        DevelopmentDogSeeder developmentDogSeeder,
        IDogOwnershipService dogOwnershipService,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _authService = authService;
        _developmentDogSeeder = developmentDogSeeder;
        _dogOwnershipService = dogOwnershipService;
        _environment = environment;
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

    public async Task<Result<Response>> UpdateDogAsync(ClaimsPrincipal principal, Guid dogId, UpdateRequest request, CancellationToken cancellationToken)
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

        var dog = await _dbContext.Dogs
            .Include(x => x.WalkerDogs)
                .ThenInclude(x => x.Walker)
            .FirstOrDefaultAsync(x => x.DogID == dogId, cancellationToken);

        var ownerLink = dog?.WalkerDogs.FirstOrDefault(x => x.Walker.UserID == user.Id);
        if (dog is null || ownerLink is null)
        {
            return Result<Response>.Failure(StatusCodes.Status404NotFound, "That dog could not be found.");
        }

        dog.Name = request.Name.Trim();
        dog.Breed = string.IsNullOrWhiteSpace(request.Breed) ? null : request.Breed.Trim();
        dog.BirthDate = request.BirthDate;
        dog.WeightPounds = request.WeightPounds;
        dog.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(dog.ToResponse(dog.WalkerDogs.Count, ownerLink.IsPrimaryOwner));
    }

    public async Task<Result<Response>> UploadAvatarAsync(ClaimsPrincipal principal, Guid dogId, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Result<Response>.Validation(new Dictionary<string, string[]>
            {
                [nameof(file)] = ["Choose an image to upload."]
            });
        }

        if (file.Length > MaxAvatarBytes)
        {
            return Result<Response>.Validation(new Dictionary<string, string[]>
            {
                [nameof(file)] = ["Dog photos must be 5 MB or smaller."]
            });
        }

        var extension = GetAvatarExtension(file.ContentType);
        if (extension is null)
        {
            return Result<Response>.Validation(new Dictionary<string, string[]>
            {
                [nameof(file)] = ["Dog photos must be JPG, PNG, WebP, or GIF images."]
            });
        }

        var hasAllowedSignature = await HasAllowedAvatarSignatureAsync(file, extension, cancellationToken);
        if (!hasAllowedSignature)
        {
            return Result<Response>.Validation(new Dictionary<string, string[]>
            {
                [nameof(file)] = ["That file does not look like a valid dog photo."]
            });
        }

        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<Response>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var dog = await _dbContext.Dogs
            .Include(x => x.WalkerDogs)
                .ThenInclude(x => x.Walker)
            .FirstOrDefaultAsync(x => x.DogID == dogId, cancellationToken);

        var ownerLink = dog?.WalkerDogs.FirstOrDefault(x => x.Walker.UserID == user.Id);
        if (dog is null || ownerLink is null)
        {
            return Result<Response>.Failure(StatusCodes.Status404NotFound, "That dog could not be found.");
        }

        var previousAvatarImage = dog.AvatarImage;
        var avatarImage = await SaveAvatarAsync(file, extension, cancellationToken);
        dog.AvatarImage = avatarImage;

        await _dbContext.SaveChangesAsync(cancellationToken);
        DeleteLocalAvatar(previousAvatarImage);

        return Result<Response>.Success(dog.ToResponse(dog.WalkerDogs.Count, ownerLink.IsPrimaryOwner));
    }

    public async Task<Result<bool>> DeleteDogAsync(ClaimsPrincipal principal, Guid dogId, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<bool>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var dog = await _dbContext.Dogs
            .Include(x => x.WalkerDogs)
                .ThenInclude(x => x.Walker)
            .FirstOrDefaultAsync(x => x.DogID == dogId, cancellationToken);

        var ownerLink = dog?.WalkerDogs.FirstOrDefault(x => x.Walker.UserID == user.Id);
        if (dog is null || ownerLink is null)
        {
            return Result<bool>.Failure(StatusCodes.Status404NotFound, "That dog could not be found.");
        }

        var avatarImage = dog.AvatarImage;
        if (ownerLink.IsPrimaryOwner)
        {
            _dbContext.Dogs.Remove(dog);
        }
        else
        {
            _dbContext.WalkerDogs.Remove(ownerLink);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (ownerLink.IsPrimaryOwner)
        {
            DeleteLocalAvatar(avatarImage);
        }

        return Result<bool>.Success(true, StatusCodes.Status204NoContent);
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
            .Where(x => x.ActivityDogs.Any(y => y.DogID == dogId))
            .OrderByDescending(x => x.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var weeklyActivities = allActivities
            .Where(x => x.StartedAtUtc >= weekStart)
            .ToList();

        var weeklyMiles = weeklyActivities.Sum(x => x.DistanceMiles);
        var weeklyAdventures = weeklyActivities.Count;
        var weeklyOutsideMinutes = weeklyActivities.Sum(x => x.DurationMinutes);
        var streakDays = CalculateStreakDays(allActivities, now);

        var ranking = await _dbContext.ActivityDogs
            .AsNoTracking()
            .Where(x => x.Activity.StartedAtUtc >= weekStart)
            .GroupBy(x => x.DogID)
            .Select(x => new
            {
                DogID = x.Key,
                Miles = x.Sum(y => y.Activity.DistanceMiles)
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

    private static string? GetAvatarExtension(string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => null
        };

    private static async Task<bool> HasAllowedAvatarSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header, cancellationToken);

        return extension switch
        {
            ".jpg" => bytesRead >= 3 &&
                header[0] == 0xff &&
                header[1] == 0xd8 &&
                header[2] == 0xff,
            ".png" => bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4e &&
                header[3] == 0x47 &&
                header[4] == 0x0d &&
                header[5] == 0x0a &&
                header[6] == 0x1a &&
                header[7] == 0x0a,
            ".gif" => bytesRead >= 6 &&
                header[0] == 0x47 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x38 &&
                (header[4] == 0x37 || header[4] == 0x39) &&
                header[5] == 0x61,
            ".webp" => bytesRead >= 12 &&
                header[0] == 0x52 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x46 &&
                header[8] == 0x57 &&
                header[9] == 0x45 &&
                header[10] == 0x42 &&
                header[11] == 0x50,
            _ => false
        };
    }

    private async Task<string> SaveAvatarAsync(IFormFile file, string extension, CancellationToken cancellationToken)
    {
        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadDirectory = Path.Combine(webRootPath, "uploads", "dogs");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/dogs/{fileName}";
    }

    private void DeleteLocalAvatar(string? avatarImage)
    {
        if (string.IsNullOrWhiteSpace(avatarImage) ||
            !avatarImage.StartsWith("/uploads/dogs/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(avatarImage);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var filePath = Path.Combine(webRootPath, "uploads", "dogs", fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
