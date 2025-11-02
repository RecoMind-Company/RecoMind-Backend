using Authentication.Core.DTOs;
using Authentication.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RecoMindAuthenticationAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticationService authenticationService, IVerificationService verificationService) : ControllerBase
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
    [HttpPost("forget-password")]
    public async Task<ActionResult<BaseToReturnDto>> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
    {
        var error = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(error);
        var result = await authenticationService.ForgetPassword(forgetPasswordDto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result);
    }
    [HttpPost("reset-password")]
    [Authorize]
    public async Task<ActionResult<BaseToReturnDto>> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var result = await authenticationService.ResetPassword(resetPasswordDto, userEmail);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result);
    }
    [HttpPost("verify")]
    public async Task<ActionResult<BaseToReturnDto>> VerifyCode(VerifyCodeDto verifyDto)
    {
        var error = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(error);
        // Code validation
        var codeMessage = await verificationService.IsCodeValid(verifyDto.Code, verifyDto.Email);
        if (!codeMessage.Success)
            return BadRequest(codeMessage.Message);
        // Password validation
        var passwordUpdate = await authenticationService.UpdatePassword(verifyDto);
        if (!passwordUpdate.Success)
            return NotFound(passwordUpdate.Message);
        return Ok(passwordUpdate);
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
