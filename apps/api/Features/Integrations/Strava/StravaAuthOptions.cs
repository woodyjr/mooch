namespace Mooch.Api.Features.Integrations.Strava;

public sealed class StravaAuthOptions
{
    public const string SectionName = "StravaAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string FrontendRedirectUri { get; set; } = "http://localhost:5173/app/walk";
}
