using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Users;

namespace Mooch.Api.Entities.Integrations;

public sealed class ConnectedAccount
{
    public Guid ConnectedAccountID { get; set; } = Guid.NewGuid();
    public Guid UserID { get; set; }
    public ConnectedAccountProvider Provider { get; set; }
    public string ExternalUserID { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTimeOffset? AccessTokenExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Activity> Activities { get; set; } = [];
    public AppUser User { get; set; } = null!;
}
