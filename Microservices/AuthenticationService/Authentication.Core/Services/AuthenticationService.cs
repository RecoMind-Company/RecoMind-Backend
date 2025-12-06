using Authentication.Core.Constants;
using Authentication.Core.DTOs;
using Authentication.Core.Helpers;
using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Core.Services;

public interface IAuthenticationService
{
    Task<AuthenticationDto> Register(RegisterDto registerDto);
    Task<AuthenticationDto> Login(LoginDto loginDto);
    Task<AuthenticationDto> GenerateNewRefreshToken(string token);
    Task<BaseToReturnDto> ForgetPassword(ForgetPasswordDto forgetPasswordDto);
    Task<BaseToReturnDto> UpdatePassword(VerifyCodeDto dto);
    Task<BaseToReturnDto> ResetPassword(ResetPasswordDto resetPasswordDto, string email);
    Task<UserToReturnDto> GetUserById(string id);
    Task<UsersToReturnDto> GetUsersByIds(List<string> ids);

}

public class AuthenticationService(UserManager<AppUser> userManager,
                                   PasswordHasher<AppUser> passwordHasher,
                                   IOptions<JwtSettings> jwtOptions,
                                   IVerificationEmailService sendEmailService,
                                   IEmailSender emailSender,
                                   IGrpcInvitationService invitationService,
                                   IGrpcTeamService grpcTeamService) : IAuthenticationService
{
    private readonly JwtSettings jwtSettings = jwtOptions.Value;
    public async Task<AuthenticationDto> Register(RegisterDto registerDto)
    {
        // Check if user exsited
        var IsUserExsited = await userManager.FindByEmailAsync(registerDto.Email);
        if (IsUserExsited is not null)
            return new AuthenticationDto { message = "This user is already registered!" };
        if (!Roles.AllRoles.Contains(registerDto.Role))
            return new AuthenticationDto { message = "Role is not valid!" };
        // Create New user 
        var user = new AppUser();

        // FOR ALL ROLES
        user.Email = registerDto.Email;
        user.UserName = new MailAddress(registerDto.Email).User;

        // FOR NON ADMIN ROLES
        if (registerDto.Role != Roles.Admin)
        {
            registerDto.Password = SecurePasswordGenerator.GenPassword();
            user.FullName = new MailAddress(registerDto.Email).User;
        }

        // FOR ADMIN ROLE
        else
            user.FullName = registerDto.FullName;

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = "";
            foreach (var error in result.Errors)
            {
                errors += $"{error.Description}, ";
            }
            return new AuthenticationDto { message = errors };
        }
        await userManager.AddToRoleAsync(user, registerDto.Role);
        // Prepare the email
        if (registerDto.Role == Roles.TeamLeader || registerDto.Role == Roles.Employee)
        {
            var subject = "recomind.com || Account Information";
            var body = $@"
                Hello,

                Here are your account details:

                Email: {user.Email}
                Password: {registerDto.Password}
                Role: {registerDto.Role}

                Please keep this information confidential.

                Best regards,  
                recomind.com Team";
            await emailSender.SendEmailAsync(user.Email!, subject, body);
        }
        // Create token and refresh token
        var token = await CreateToken(user);
        var refreshToken = GenerateRefershToken();
        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);
        return new AuthenticationDto
        {
            Name = user.FullName,
            Email = user.Email,
            IsAuthenticated = true,
            message = "completed sucessfully!",
            ExperiesOn = token.ValidTo,
            Roles = new List<string> { registerDto.Role }, // To Edit --------------------------------------------
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            RefreshTokenExp = refreshToken.ExpiresOn
        };
    }

    public async Task<AuthenticationDto> Login(LoginDto loginDto)
    {
        // get user by it's email 
        // check if the user exist
        // check if the paasword is correct
        // Create Token
        // Get roles
        // create the AuthentiactionDto to return
        // check if the user have any refresh tokens is active
        // if not create refresh token and add it to datebase then update the user
        var userToReturn = new AuthenticationDto();
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            userToReturn.message = "email or password is incorrect";
            return userToReturn;
        }
        // CHECK IF THE USER ROLE IS ADMIN //
        var userRoles = await userManager.GetRolesAsync(user);
        if (userRoles.Contains(Roles.Admin))
        {
            // Continue the normal login proccess
        }
        else
        {
            var validInvitation = await invitationService.LoginAttempt(user.Email!);
            if (!validInvitation.Success)
            {
                userToReturn.message = validInvitation.Message;
                return userToReturn;
            }
        }
        var userTeam = await grpcTeamService.GetTeamByUserId(user.Id);
        var token = await CreateToken(user, userTeam.CompanyId);
        userToReturn.Name = user.FullName;
        userToReturn.Email = user.Email;
        userToReturn.IsAuthenticated = true;
        userToReturn.ExperiesOn = token.ValidTo;
        userToReturn.Token = new JwtSecurityTokenHandler().WriteToken(token);
        userToReturn.message = "login successfully";
        userToReturn.Roles = userRoles.ToList();
        // photoUrl
        if (user.RefreshTokens.Any(x => x.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.First(z => z.IsActive);
            userToReturn.RefreshToken = activeRefreshToken.Token;
            userToReturn.RefreshTokenExp = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = GenerateRefershToken();
            userToReturn.RefreshToken = refreshToken.Token;
            userToReturn.RefreshTokenExp = refreshToken.ExpiresOn;
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);
        }
        return userToReturn;
    }

    public async Task<AuthenticationDto> GenerateNewRefreshToken(string token)
    {
        // get user with this refresh token
        // check if user exist
        // check if the token is active
        // revoke the old token
        // genereate an new refresh token
        // add to table and update user
        // generate new jwt token
        var userToReturn = new AuthenticationDto();
        var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token));
        if (user is null)
        {
            userToReturn.message = "Invalid token";
            return userToReturn;
        }

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
        if (!refreshToken.IsActive)
        {
            userToReturn.message = "Inactive token";
            return userToReturn;
        }

        refreshToken.RevokeOn = DateTime.UtcNow;

        var newRefreshToken = GenerateRefershToken();
        user.RefreshTokens.Add(newRefreshToken);
        await userManager.UpdateAsync(user);

        var userTeam = await grpcTeamService.GetTeamByUserId(user.Id);
        var jwtToken = await CreateToken(user, userTeam.CompanyId);
        var roles = await userManager.GetRolesAsync(user);

        userToReturn.IsAuthenticated = true;
        userToReturn.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        userToReturn.ExperiesOn = jwtToken.ValidTo;
        userToReturn.RefreshToken = newRefreshToken.Token;
        userToReturn.RefreshTokenExp = newRefreshToken.ExpiresOn;
        userToReturn.Email = user.Email;
        userToReturn.Name = user.FullName;
        userToReturn.Roles = roles.ToList();
        userToReturn.message = "refresh token created successfully";
        return userToReturn;
    }

    public async Task<BaseToReturnDto> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
    {
        var user = await userManager.FindByEmailAsync(forgetPasswordDto.Email);
        if (user is null)
            return new BaseToReturnDto { Message = "Email Is NotFound" };
        await sendEmailService.SendVerificationCodeEmail(forgetPasswordDto.Email);
        return new BaseToReturnDto { Success = true, Message = "Email send succssfully" };
    }

    public async Task<BaseToReturnDto> UpdatePassword(VerifyCodeDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return new BaseToReturnDto { Message = "User is not found" };

        var HashPassword = passwordHasher.HashPassword(user, dto.NewPassword);
        user.PasswordHash = HashPassword;
        await userManager.UpdateAsync(user);
        return new BaseToReturnDto { Success = true, Message = "The password Updated successfully" };

    }

    public async Task<BaseToReturnDto> ResetPassword(ResetPasswordDto resetPasswordDto, string email)
    {
        //get user by email
        // Compare the resetPasswordDto.oldPassword and the acutal password
        // if wrong return message
        // if correct hash the new password and update the user 
        var user = await userManager.FindByEmailAsync(email);
        var IsPasswordCorrect = await userManager.CheckPasswordAsync(user, resetPasswordDto.OldPassword);
        if (!IsPasswordCorrect)
            return new BaseToReturnDto { Message = "Password is not correct" };
        var HashPassword = passwordHasher.HashPassword(user, resetPasswordDto.NewPassword);
        user.PasswordHash = HashPassword;
        await userManager.UpdateAsync(user);
        return new BaseToReturnDto { Success = true, Message = "The password Updated successfully" };
    }

    private async Task<JwtSecurityToken> CreateToken(AppUser user, string compnayId = null) // null comany id 
    {
        var Claims = new List<Claim>();
        Claims.Add(new Claim("", JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        Claims.Add(new Claim(ClaimTypes.Email, user.Email));
        Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        Claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        if (compnayId is not null)
            Claims.Add(new Claim("companyId", compnayId));
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            Claims.Add(new(ClaimTypes.Role, role));


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var crd = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: Claims,
            expires: DateTime.Now.AddHours(jwtSettings.DurationInHours),
            signingCredentials: crd);

        return token;
    }
    private RefreshToken GenerateRefershToken()
    {
        var RandomNum = new byte[32];
        using var generator = new RNGCryptoServiceProvider();
        generator.GetBytes(RandomNum);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNum),
            ExpiresOn = DateTime.UtcNow.AddDays(2),
            CreatedOn = DateTime.UtcNow,
        };
    }

    public async Task<UserToReturnDto> GetUserById(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return null;
        var userToReturn = new UserToReturnDto
        {
            Email = user.Email!,
            Id = user.Id,
            Role = (await userManager.GetRolesAsync(user)).FirstOrDefault()!
        };
        return userToReturn;

    }

    public async Task<UsersToReturnDto> GetUsersByIds(List<string> ids)
    {
        var userNamesList = await userManager.Users
             .Where(u => ids.Contains(u.Id))
             .Select(u => u.UserName)
             .ToListAsync();
        var usersToReturnDto = new UsersToReturnDto
        {
            UserNames = userNamesList
        };
        return usersToReturnDto;
    }
}



