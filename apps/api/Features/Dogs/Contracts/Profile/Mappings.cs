using Mooch.Api.Entities.Dogs;

namespace Mooch.Api.Features.Dogs.Contracts.Profile;

public static class Mappings
{
    public static Response ToResponse(this Dog dog, int ownerCount, bool isPrimaryOwner) =>
        new()
        {
            Id = dog.DogID,
            Name = dog.Name,
            Breed = dog.Breed,
            BirthDate = dog.BirthDate,
            WeightPounds = dog.WeightPounds,
            Bio = dog.Bio,
            AvatarImage = dog.AvatarImage,
            CreatedDateUtc = dog.CreatedDateUtc,
            OwnerCount = ownerCount,
            IsPrimaryOwner = isPrimaryOwner
        };
}

