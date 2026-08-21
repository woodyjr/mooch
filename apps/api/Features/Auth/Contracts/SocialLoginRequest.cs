namespace Mooch.Api.Features.Auth.Contracts;

public sealed class SocialLoginRequest
{
    public string Provider { get; set; } = "google";
    public string Credential { get; set; } = string.Empty;
}

