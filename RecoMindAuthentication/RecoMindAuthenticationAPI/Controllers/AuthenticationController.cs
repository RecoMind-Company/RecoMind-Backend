using Authentication.Core.DTOs;
using Authentication.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace RecoMindAuthenticationAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationDto>> Register(RegisterDto registerDto)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await authenticationService.Register(registerDto);
        if (!result.IsAuthenticated)
            return BadRequest(result.message);
        // refresh token
        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExp);
        return Ok(result);
    }
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationDto>> Login(LoginDto loginDto)
    {
        var error = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(error);
        var result = await authenticationService.Login(loginDto);
        if (!result.IsAuthenticated)
            return BadRequest(result.message);
        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExp);
        return Ok(result);
    }
    [HttpGet("refresh-token")]
    public async Task<ActionResult<AuthenticationDto>> CreateRefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var result = await authenticationService.GenerateNewRefreshToken(refreshToken);
        if (!result.IsAuthenticated)
            return BadRequest(result.message);
        SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExp);
        return Ok(result);
    }

    private void SetRefreshTokenInCookies(string refreshToken, DateTime exp)
    {
        var cookiesOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = exp.ToLocalTime(),
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookiesOptions);
    }
}
