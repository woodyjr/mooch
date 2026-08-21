namespace Mooch.Api.Features.Dogs.Contracts.Profile;

public sealed class CreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? WeightPounds { get; set; }
    public string? Bio { get; set; }
    public string? AvatarImage { get; set; }
    public string? CoOwnerEmail { get; set; }
}

