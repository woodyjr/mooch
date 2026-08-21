namespace Mooch.Api.Features.Auth.Contracts;

public sealed class Response
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Provider { get; init; } = "email";
}

