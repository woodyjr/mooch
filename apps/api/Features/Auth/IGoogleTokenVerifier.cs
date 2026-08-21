namespace Mooch.Api.Features.Auth;

public interface IGoogleTokenVerifier
{
    Task<GoogleIdentity> VerifyAsync(string credential, CancellationToken cancellationToken);
}

