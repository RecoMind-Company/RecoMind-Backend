using Authentication.Core.DTOs;
using Authentication.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RecoMindAuthenticationAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<BaseToReturnDto>> UpdateProfile([FromForm] ProfileDto profile)
        {
            var errors = ModelState.Values.SelectMany(e => e.Errors);
            if (!ModelState.IsValid)
                return BadRequest(errors);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var result = await accountService.EditProfile(profile, userEmail);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ProfileToReturnDto>> GetProfile()
        {
            var errors = ModelState.Values.SelectMany(e => e.Errors);
            if (!ModelState.IsValid)
                return BadRequest(errors);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await accountService.GetProfile(email);
            if (result is null)
                return NotFound("this user is not found");
            return Ok(result);
        }
    }
}
