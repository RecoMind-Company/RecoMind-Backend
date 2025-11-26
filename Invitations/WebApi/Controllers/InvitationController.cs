using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationController(IInvitationService invitationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BaseToReturnDto>> SendInvitation(InvitationDto invitation)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await invitationService.SendInvitationAsync(invitation);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
