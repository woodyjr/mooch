namespace Mooch.Api.Features.Auth.Contracts;

public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? AvatarImage { get; set; }
}

