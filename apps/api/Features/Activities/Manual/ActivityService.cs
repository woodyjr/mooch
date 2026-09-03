using System.Security.Claims;
using Mooch.Api.Data;
using Mooch.Api.Entities.Activities;
using Mooch.Api.Features.Activities.Manual.Contracts;
using Mooch.Api.Features.Auth;
using Mooch.Api.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Mooch.Api.Features.Activities.Manual;

public sealed class ActivityService : IActivityService
{
    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;

    public ActivityService(AppDbContext dbContext, IAuthService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    public async Task<Result<Response>> CreateManualAsync(ClaimsPrincipal principal, CreateRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = new Dictionary<string, string[]>();
        var distinctDogIds = request.DogIDs
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (distinctDogIds.Length == 0)
        {
            validationErrors[nameof(request.DogIDs)] = ["Choose at least one dog for this adventure."];
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            validationErrors[nameof(request.Title)] = ["Title is required."];
        }

        if (request.DistanceMiles <= 0)
        {
            validationErrors[nameof(request.DistanceMiles)] = ["Distance needs to be greater than zero."];
        }

        if (request.DurationMinutes <= 0)
        {
            validationErrors[nameof(request.DurationMinutes)] = ["Duration needs to be greater than zero."];
        }

        if (request.StartedAtUtc == default)
        {
            validationErrors[nameof(request.StartedAtUtc)] = ["Choose when this adventure happened."];
        }

        if (validationErrors.Count > 0)
        {
            return Result<Response>.Validation(validationErrors);
        }

        var user = await _authService.GetUserAsync(principal);
        if (user is null)
        {
            return Result<Response>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var walker = await _authService.EnsureWalkerAsync(user, cancellationToken);
        var availableDogIds = await _dbContext.WalkerDogs
            .AsNoTracking()
            .Where(x => x.WalkerID == walker.WalkerID)
            .Select(x => x.DogID)
            .ToListAsync(cancellationToken);

        if (distinctDogIds.Any(x => !availableDogIds.Contains(x)))
        {
            return Result<Response>.Failure(StatusCodes.Status404NotFound, "One or more selected dogs are not part of your pack.");
        }

        var activity = new Activity
        {
            WalkerID = walker.WalkerID,
            Title = request.Title.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            DistanceMiles = decimal.Round(request.DistanceMiles, 2, MidpointRounding.AwayFromZero),
            DurationMinutes = request.DurationMinutes,
            StartedAtUtc = request.StartedAtUtc,
            Source = ActivitySourceType.Manual,
            ActivityDogs = distinctDogIds
                .Select(dogId => new ActivityDog
                {
                    DogID = dogId
                })
                .ToList()
        };

        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response
        {
            ActivityID = activity.ActivityID,
            DogIDs = activity.ActivityDogs.Select(x => x.DogID).ToArray(),
            WalkerID = activity.WalkerID,
            Title = activity.Title,
            Notes = activity.Notes,
            DistanceMiles = activity.DistanceMiles,
            DurationMinutes = activity.DurationMinutes,
            StartedAtUtc = activity.StartedAtUtc
        }, StatusCodes.Status201Created);
    }
}
