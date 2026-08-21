using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Mooch.Api.Features.Auth;

public sealed class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly GoogleAuthOptions _options;

    public GoogleTokenVerifier(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleIdentity> VerifyAsync(string credential, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new InvalidOperationException("GoogleAuth:ClientId is missing from API configuration.");
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(
            credential,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            });

        return new GoogleIdentity(
            payload.Subject,
            payload.Email ?? string.Empty,
            payload.EmailVerified,
            payload.Name,
            payload.Picture);
    }
}

