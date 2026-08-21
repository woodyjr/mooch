using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Auth;
using Mooch.Api.Features.Auth.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mooch.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<Response>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(User, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<Response>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Response>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("social")]
    public async Task<ActionResult<Response>> SocialLogin(SocialLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SocialLoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("social/register")]
    public async Task<ActionResult<Response>> SocialRegister(SocialLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SocialRegisterAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return NoContent();
    }
}

