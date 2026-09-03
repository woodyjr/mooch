using Mooch.Api.Infrastructure.Http;
using Mooch.Api.Features.Dogs.Contracts.Ownership;
using Mooch.Api.Features.Dogs.Contracts.Profile;
using Mooch.Api.Features.Dogs.Ownership;
using Mooch.Api.Features.Dogs.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mooch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DogsController : ControllerBase
{
    private readonly IDogOwnershipService _dogOwnershipService;
    private readonly IDogProfileService _dogProfileService;

    public DogsController(IDogProfileService dogProfileService, IDogOwnershipService dogOwnershipService)
    {
        _dogProfileService = dogProfileService;
        _dogOwnershipService = dogOwnershipService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Response>>> GetDogs(CancellationToken cancellationToken)
    {
        var result = await _dogProfileService.GetDogsAsync(User, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{dogId:guid}/dashboard")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(Guid dogId, CancellationToken cancellationToken)
    {
        var result = await _dogProfileService.GetDashboardAsync(User, dogId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response>> CreateDog(CreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _dogProfileService.CreateDogAsync(User, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{dogId:guid}")]
    public async Task<ActionResult<Response>> UpdateDog(Guid dogId, UpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _dogProfileService.UpdateDogAsync(User, dogId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{dogId:guid}/avatar")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<Response>> UploadAvatar(Guid dogId, [FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        var result = await _dogProfileService.UploadAvatarAsync(User, dogId, file, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{dogId:guid}")]
    public async Task<ActionResult<bool>> DeleteDog(Guid dogId, CancellationToken cancellationToken)
    {
        var result = await _dogProfileService.DeleteDogAsync(User, dogId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{dogId:guid}/owner-invites")]
    public async Task<ActionResult<IReadOnlyList<OwnerInviteResponse>>> GetOwnerInvites(Guid dogId, CancellationToken cancellationToken)
    {
        var result = await _dogOwnershipService.GetOwnerInvitesAsync(User, dogId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("owner-invites/pending")]
    public async Task<ActionResult<IReadOnlyList<PendingOwnerInviteResponse>>> GetPendingOwnerInvites(CancellationToken cancellationToken)
    {
        var result = await _dogOwnershipService.GetPendingOwnerInvitesAsync(User, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{dogId:guid}/owner-invites")]
    public async Task<ActionResult<OwnerInviteResponse>> InviteOwner(Guid dogId, InviteOwnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _dogOwnershipService.InviteOwnerAsync(User, dogId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("owner-invites/{inviteId:guid}/accept")]
    public async Task<ActionResult<OwnerInviteResponse>> AcceptOwnerInvite(Guid inviteId, CancellationToken cancellationToken)
    {
        var result = await _dogOwnershipService.AcceptOwnerInviteAsync(User, inviteId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("owner-invites/{inviteId:guid}/decline")]
    public async Task<ActionResult<OwnerInviteResponse>> DeclineOwnerInvite(Guid inviteId, CancellationToken cancellationToken)
    {
        var result = await _dogOwnershipService.DeclineOwnerInviteAsync(User, inviteId, cancellationToken);
        return this.ToActionResult(result);
    }
}
