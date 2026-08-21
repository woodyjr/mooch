namespace Mooch.Api.Features.Dogs.Contracts.Ownership;

public sealed class PendingOwnerInviteResponse
{
    public Guid Id { get; init; }
    public Guid DogID { get; init; }
    public string DogName { get; init; } = string.Empty;
    public string InvitedByDisplayName { get; init; } = string.Empty;
    public DateTimeOffset CreatedDateUtc { get; init; }
}

