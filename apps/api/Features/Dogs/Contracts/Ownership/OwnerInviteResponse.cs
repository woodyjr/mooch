namespace Mooch.Api.Features.Dogs.Contracts.Ownership;

public sealed class OwnerInviteResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedDateUtc { get; init; }
}

