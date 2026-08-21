namespace Mooch.Api.Features.Dogs.Contracts.Profile;

public sealed class Response
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Breed { get; init; }
    public DateOnly? BirthDate { get; init; }
    public decimal? WeightPounds { get; init; }
    public string? Bio { get; init; }
    public string? AvatarImage { get; init; }
    public DateTimeOffset CreatedDateUtc { get; init; }
    public int OwnerCount { get; init; }
    public bool IsPrimaryOwner { get; init; }
}

