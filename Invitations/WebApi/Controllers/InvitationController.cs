using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationController(IInvitationService invitationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BaseToReturnDto>> SendInvitation(SendInvitationDto invitation)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        invitation.SenderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await invitationService.SendInvitationAsync(invitation);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<InvitationsToReturnDto>> GetInvitationById(int id)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var response = await invitationService.GetInvitationByIdAsync(id);
        if (response == null)
            return NotFound($"There is no invitation with id : {id}");
        return Ok(response);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvitationsToReturnDto>>> GetInvitaions([FromQuery] GetInvitationDto invitationDto)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await invitationService.GetInvitationsByStatus(invitationDto);
        return Ok(result);

    }
}
