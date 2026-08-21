using Mooch.Api.Entities.Integrations;

namespace Mooch.Api.Features.Auth;

public static class ExternalAuthProvider
{
    public const string Google = "google";
    public const string Apple = "apple";
    public const string Email = "email";

    public static string Normalize(string? provider)
    {
        return (provider ?? Email).Trim().ToLowerInvariant() switch
        {
            Google => Google,
            Apple => Apple,
            _ => Email
        };
    }

    public static ConnectedAccountProvider ToConnectedAccountProvider(string? provider)
    {
        return Normalize(provider) switch
        {
            Google => ConnectedAccountProvider.Google,
            Apple => ConnectedAccountProvider.Apple,
            _ => throw new InvalidOperationException($"Provider '{provider}' does not map to a connected account provider.")
        };
    }
}


