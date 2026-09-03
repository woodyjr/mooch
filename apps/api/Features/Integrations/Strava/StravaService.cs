using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using Mooch.Api.Data;
using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Integrations;
using Mooch.Api.Entities.Users;
using Mooch.Api.Features.Integrations.Strava.Contracts;
using Mooch.Api.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mooch.Api.Features.Integrations.Strava;

public sealed class StravaService : IStravaService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly StravaAuthOptions _options;

    public StravaService(
        HttpClient httpClient,
        AppDbContext dbContext,
        UserManager<AppUser> userManager,
        IOptions<StravaAuthOptions> options)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _userManager = userManager;
        _options = options.Value;
    }

    public string CreateAuthorizationUrl(string state)
    {
        EnsureConfigured();

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = _options.RedirectUri,
            ["approval_prompt"] = "auto",
            ["scope"] = "read,activity:read_all",
            ["state"] = state
        };

        return $"https://www.strava.com/oauth/authorize?{ToQueryString(parameters)}";
    }

    public async Task<Result<bool>> CompleteConnectionAsync(ClaimsPrincipal principal, string code, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result<bool>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var exchange = await ExchangeAuthorizationCodeAsync(code, cancellationToken);
        if (exchange is null || exchange.Athlete?.Id is null)
        {
            return Result<bool>.Failure(StatusCodes.Status502BadGateway, "Could not complete the Strava connection.");
        }

        var externalUserId = exchange.Athlete.Id.Value.ToString(CultureInfo.InvariantCulture);
        var connectedAccount = await _dbContext.ConnectedAccounts
            .FirstOrDefaultAsync(
                x => x.UserID == user.Id && x.Provider == ConnectedAccountProvider.Strava,
                cancellationToken);

        if (connectedAccount is null)
        {
            connectedAccount = new ConnectedAccount
            {
                UserID = user.Id,
                Provider = ConnectedAccountProvider.Strava,
                ExternalUserID = externalUserId,
                AccessToken = exchange.AccessToken,
                RefreshToken = exchange.RefreshToken,
                AccessTokenExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(exchange.ExpiresAt)
            };

            _dbContext.ConnectedAccounts.Add(connectedAccount);
        }
        else
        {
            connectedAccount.ExternalUserID = externalUserId;
            connectedAccount.AccessToken = exchange.AccessToken;
            connectedAccount.RefreshToken = exchange.RefreshToken;
            connectedAccount.AccessTokenExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(exchange.ExpiresAt);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<StravaStatusResponse>> GetStatusAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result<StravaStatusResponse>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var connectedAccount = await _dbContext.ConnectedAccounts
            .FirstOrDefaultAsync(
                x => x.UserID == user.Id && x.Provider == ConnectedAccountProvider.Strava,
                cancellationToken);

        if (connectedAccount is null)
        {
            return Result<StravaStatusResponse>.Success(new StravaStatusResponse());
        }

        var pendingImportCount = await _dbContext.ImportedActivities
            .CountAsync(x => x.ConnectedAccountID == connectedAccount.ConnectedAccountID && x.RequiresDogAssignment, cancellationToken);

        var lastImportedActivityAtUtc = await _dbContext.ImportedActivities
            .Where(x => x.ConnectedAccountID == connectedAccount.ConnectedAccountID)
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => (DateTimeOffset?)x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<StravaStatusResponse>.Success(new StravaStatusResponse
        {
            IsConnected = true,
            PendingImportCount = pendingImportCount,
            LastImportedActivityAtUtc = lastImportedActivityAtUtc
        });
    }

    public async Task<Result<IReadOnlyList<ImportedActivityResponse>>> GetImportedActivitiesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result<IReadOnlyList<ImportedActivityResponse>>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var connectedAccount = await _dbContext.ConnectedAccounts
            .FirstOrDefaultAsync(
                x => x.UserID == user.Id && x.Provider == ConnectedAccountProvider.Strava,
                cancellationToken);

        if (connectedAccount is null)
        {
            return Result<IReadOnlyList<ImportedActivityResponse>>.Success([]);
        }

        var imports = await _dbContext.ImportedActivities
            .Where(x => x.ConnectedAccountID == connectedAccount.ConnectedAccountID)
            .OrderByDescending(x => x.StartedAtUtc)
            .Take(20)
            .Select(x => new ImportedActivityResponse
            {
                ImportedActivityID = x.ImportedActivityID,
                Title = x.Title,
                ActivityType = x.ActivityType,
                DistanceMiles = x.DistanceMiles,
                DurationMinutes = x.DurationMinutes,
                StartedAtUtc = x.StartedAtUtc,
                RequiresDogAssignment = x.RequiresDogAssignment
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ImportedActivityResponse>>.Success(imports);
    }

    public async Task<Result<StravaSyncResponse>> SyncAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result<StravaSyncResponse>.Failure(StatusCodes.Status401Unauthorized, "You are not signed in.");
        }

        var walker = await _dbContext.Walkers.FirstOrDefaultAsync(x => x.UserID == user.Id, cancellationToken);
        if (walker is null)
        {
            return Result<StravaSyncResponse>.Failure(StatusCodes.Status409Conflict, "Create your Mooch walker profile before syncing Strava.");
        }

        var connectedAccount = await _dbContext.ConnectedAccounts
            .FirstOrDefaultAsync(
                x => x.UserID == user.Id && x.Provider == ConnectedAccountProvider.Strava,
                cancellationToken);

        if (connectedAccount is null)
        {
            return Result<StravaSyncResponse>.Failure(StatusCodes.Status404NotFound, "Connect Strava before syncing activities.");
        }

        await EnsureFreshAccessTokenAsync(connectedAccount, cancellationToken);

        var newestImportedAt = await _dbContext.ImportedActivities
            .Where(x => x.ConnectedAccountID == connectedAccount.ConnectedAccountID)
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => (DateTimeOffset?)x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var after = newestImportedAt?.AddDays(-1) ?? DateTimeOffset.UtcNow.AddDays(-60);
        var activities = await FetchActivitiesAsync(connectedAccount.AccessToken, after, cancellationToken);

        var importedCount = 0;
        var skippedCount = 0;

        foreach (var activity in activities.Where(IsSupportedActivity))
        {
            var externalActivityId = activity.Id.ToString(CultureInfo.InvariantCulture);
            var exists = await _dbContext.ImportedActivities.AnyAsync(
                x => x.ConnectedAccountID == connectedAccount.ConnectedAccountID && x.ExternalActivityID == externalActivityId,
                cancellationToken);

            if (exists)
            {
                skippedCount++;
                continue;
            }

            _dbContext.ImportedActivities.Add(new ImportedActivity
            {
                WalkerID = walker.WalkerID,
                ConnectedAccountID = connectedAccount.ConnectedAccountID,
                ExternalActivityID = externalActivityId,
                Title = string.IsNullOrWhiteSpace(activity.Name) ? $"Strava {activity.SportType}" : activity.Name.Trim(),
                ActivityType = (activity.SportType ?? activity.Type ?? "Workout").Trim(),
                DistanceMiles = ToMiles(activity.Distance),
                DurationMinutes = ToMinutes(activity.MovingTime > 0 ? activity.MovingTime : activity.ElapsedTime),
                StartedAtUtc = activity.StartDate,
                RequiresDogAssignment = true,
                Source = ActivitySourceType.Strava
            });

            importedCount++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var pendingImportCount = await _dbContext.ImportedActivities
            .CountAsync(x => x.ConnectedAccountID == connectedAccount.ConnectedAccountID && x.RequiresDogAssignment, cancellationToken);

        return Result<StravaSyncResponse>.Success(new StravaSyncResponse
        {
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            PendingImportCount = pendingImportCount
        });
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId)
            || string.IsNullOrWhiteSpace(_options.ClientSecret)
            || string.IsNullOrWhiteSpace(_options.RedirectUri))
        {
            throw new InvalidOperationException("Strava OAuth is not configured yet. Add StravaAuth settings first.");
        }
    }

    private async Task EnsureFreshAccessTokenAsync(ConnectedAccount connectedAccount, CancellationToken cancellationToken)
    {
        if (connectedAccount.AccessTokenExpiresAtUtc is null || connectedAccount.AccessTokenExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(2))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(connectedAccount.RefreshToken))
        {
            throw new InvalidOperationException("Strava refresh token is missing.");
        }

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = connectedAccount.RefreshToken
        });

        var response = await _httpClient.PostAsync("https://www.strava.com/api/v3/oauth/token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<StravaTokenResponse>(cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException("Strava refresh response was empty.");
        }

        connectedAccount.AccessToken = payload.AccessToken;
        connectedAccount.RefreshToken = payload.RefreshToken;
        connectedAccount.AccessTokenExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAt);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<StravaTokenResponse?> ExchangeAuthorizationCodeAsync(string code, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync("https://www.strava.com/api/v3/oauth/token", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<StravaTokenResponse>(cancellationToken);
    }

    private async Task<IReadOnlyList<StravaActivitySummary>> FetchActivitiesAsync(string accessToken, DateTimeOffset after, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete/activities?after={after.ToUnixTimeSeconds()}&page=1&per_page=100");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<StravaActivitySummary>>(cancellationToken) ?? [];
    }

    private static bool IsSupportedActivity(StravaActivitySummary activity)
    {
        var type = (activity.SportType ?? activity.Type ?? string.Empty).Trim();
        return type.Equals("Walk", StringComparison.OrdinalIgnoreCase)
            || type.Equals("Run", StringComparison.OrdinalIgnoreCase)
            || type.Equals("Hike", StringComparison.OrdinalIgnoreCase);
    }

    private static decimal ToMiles(double meters) => decimal.Round((decimal)meters * 0.000621371m, 2, MidpointRounding.AwayFromZero);

    private static int ToMinutes(int seconds) => Math.Max(1, (int)Math.Round(seconds / 60d, MidpointRounding.AwayFromZero));

    private static string ToQueryString(IReadOnlyDictionary<string, string> values) =>
        string.Join("&", values.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

    private sealed class StravaTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public long ExpiresAt { get; set; }
        public StravaAthleteSummary? Athlete { get; set; }
    }

    private sealed class StravaAthleteSummary
    {
        public long? Id { get; set; }
    }

    private sealed class StravaActivitySummary
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SportType { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Distance { get; set; }
        public int MovingTime { get; set; }
        public int ElapsedTime { get; set; }
        public DateTimeOffset StartDate { get; set; }
    }
}

