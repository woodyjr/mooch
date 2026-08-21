using Mooch.Api.Entities.Challenges;
using Mooch.Api.Entities.Friendships;
using Mooch.Api.Entities.Integrations;
using Mooch.Api.Entities.Walkers;
using Microsoft.AspNetCore.Identity;

namespace Mooch.Api.Entities.Users;

public sealed class AppUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public string? AvatarImage { get; set; }

    public ICollection<ChallengeParticipant> ChallengeParticipants { get; set; } = [];
    public ICollection<ConnectedAccount> ConnectedAccounts { get; set; } = [];
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = [];
    public ICollection<Friendship> SentFriendRequests { get; set; } = [];
    public Walker? Walker { get; set; }
}
