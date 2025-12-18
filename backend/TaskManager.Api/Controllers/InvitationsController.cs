using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly IUserInvitationService _invitationService;

    public InvitationsController(IUserInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserInvitation>>> GetInvitations()
    {
        var invitations = await _invitationService.GetAllAsync();
        return Ok(invitations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserInvitation>> GetInvitation(string id)
    {
        var invitation = await _invitationService.GetByIdAsync(id);
        if (invitation == null)
            return NotFound();
        return Ok(invitation);
    }

    [HttpGet("token/{token}")]
    public async Task<ActionResult<UserInvitation>> GetInvitationByToken(string token)
    {
        var invitation = await _invitationService.GetByTokenAsync(token);
        if (invitation == null)
            return NotFound("Invitation not found or expired");
        return Ok(invitation);
    }

    [HttpPost]
    public async Task<ActionResult<UserInvitation>> CreateInvitation(CreateUserInvitationDto dto)
    {
        var createdInvitation = await _invitationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetInvitation), new { id = createdInvitation.Id }, createdInvitation);
    }

    [HttpPost("accept/{token}")]
    public async Task<ActionResult<UserInvitation>> AcceptInvitation(string token, CreateUserDto userDto)
    {
        var result = await _invitationService.AcceptInvitationAsync(token, userDto);
        if (result == null)
            return BadRequest("Invalid token or user already exists");
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvitation(string id)
    {
        var result = await _invitationService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("organization/{organizationId}")]
    public async Task<ActionResult<IEnumerable<UserInvitation>>> GetInvitationsByOrganization(string organizationId)
    {
        var invitations = await _invitationService.GetByOrganizationAsync(organizationId);
        return Ok(invitations);
    }
}