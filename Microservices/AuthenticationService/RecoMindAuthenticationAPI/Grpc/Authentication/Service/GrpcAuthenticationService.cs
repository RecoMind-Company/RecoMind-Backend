
using Authentication.Core.DTOs;
using Authentication.Core.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace RecoMindAuthenticationAPI.Grpc.Authentication.Service;

public class GrpcAuthenticationService(IAuthenticationService AuthService, IVerificationService verificationService) : AuthenticationService.AuthenticationServiceBase
{
    public override async Task<AuthenticatedMessage> Register(RegisterMessage request, ServerCallContext context)
    {
        // Map message to Dto
        var registerDto = new RegisterDto
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role
        };
        // Call the service
        var response = await AuthService.Register(registerDto);
        // Check if the registration was successful
        if (!response.IsAuthenticated)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, response.message));
        }
        // Map Dto to message
        var messageToReturn = new AuthenticatedMessage
        {
            Name = response.Name ?? string.Empty,
            Email = response.Email ?? string.Empty,
            ExperiesOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(response.ExperiesOn.ToUniversalTime()),
            Token = response.Token ?? string.Empty,
            IsAuthenticated = response.IsAuthenticated,
            PhotoUrl = response.PhotoUrl ?? string.Empty,
            Message = response.message ?? string.Empty,
        };
        messageToReturn.Roles.AddRange(response.Roles);
        return messageToReturn;
    }
    public override async Task<AuthenticatedMessage> Login(LoginMessage request, ServerCallContext context)
    {
        var loginDto = new LoginDto
        {
            Email = request.Email,
            Password = request.Password
        };

        var response = await AuthService.Login(loginDto);

        if (!response.IsAuthenticated)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, response.message));
        }

        var messageToReturn = new AuthenticatedMessage
        {
            Name = response.Name ?? string.Empty,
            Email = response.Email ?? string.Empty,
            ExperiesOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(response.ExperiesOn.ToUniversalTime()),
            Token = response.Token ?? string.Empty,
            IsAuthenticated = response.IsAuthenticated,
            PhotoUrl = response.PhotoUrl ?? string.Empty,
            Message = response.message ?? string.Empty,
        };
        messageToReturn.Roles.AddRange(response.Roles);
        return messageToReturn;
    }
    public override async Task<AuthenticatedMessage> CreateRefreshToken(TokenMessage request, ServerCallContext context)
    {
        var response = await AuthService.GenerateNewRefreshToken(request.RefreshToken);
        if (!response.IsAuthenticated)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, response.message));
        }
        return new AuthenticatedMessage
        {
            Name = response.Name ?? string.Empty,
            Email = response.Email ?? string.Empty,
            ExperiesOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(response.ExperiesOn.ToUniversalTime()),
            Token = response.Token ?? string.Empty,
            IsAuthenticated = response.IsAuthenticated,
            Roles = { response.Roles },
            PhotoUrl = response.PhotoUrl ?? string.Empty,
            Message = response.message ?? string.Empty,
        };
    }
    public override async Task<BaseMessage> ForgetPassword(ForgetPasswordMessage request, ServerCallContext context)
    {
        var forgetPasswordDto = new ForgetPasswordDto { Email = request.Email };

        var response = await AuthService.ForgetPassword(forgetPasswordDto);

        if (!response.Success)
            throw new RpcException(new Status(StatusCode.InvalidArgument, response.Message));

        return new BaseMessage { Success = response.Success, Message = response.Message };
    }
    [Authorize]
    public override async Task<BaseMessage> ResetPassword(ResetPasswordMessage request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var userEmail = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var resetPasswordDto = new ResetPasswordDto
        {
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword.NewPassword_,
            ConfirmNewPassword = request.NewPassword.ConfirmNewPassword
        };
        var response = await AuthService.ResetPassword(resetPasswordDto, userEmail);
        if (!response.Success)
            throw new RpcException(new Status(StatusCode.InvalidArgument, response.Message));
        return new BaseMessage { Success = response.Success, Message = response.Message };
    }
    public override async Task<BaseMessage> VerifyCode(VerifyCodeMessage request, ServerCallContext context)
    {
        var verifyCodeDto = new VerifyCodeDto
        {
            Code = request.Code,
            Email = request.Email,
            NewPassword = request.NewPassword.NewPassword_,
            ConfirmNewPassword = request.NewPassword.ConfirmNewPassword
        };

        var codeMessage = await verificationService.IsCodeValid(verifyCodeDto.Code, verifyCodeDto.Email);
        if (!codeMessage.Success)
            throw new RpcException(new Status(StatusCode.InvalidArgument, codeMessage.Message));
        return new BaseMessage { Success = codeMessage.Success, Message = codeMessage.Message };
    }
    public override async Task<UserToReturnMessage> GetUserById(GetUserByIdMessage request, ServerCallContext context)
    {
        var user = await AuthService.GetUserById(request.UserId);
        return new UserToReturnMessage { Email = user.Email ?? string.Empty, Id = user.Id ?? string.Empty, Role = user.Role ?? string.Empty };
    }
    public override async Task<UsersToReturnMessage> GetUsers(GetUsersMessage request, ServerCallContext context)
    {
        var users = await AuthService.GetUsersByIds(request.Ids.ToList());
        if (!users.UserNames.Any())
            return new UsersToReturnMessage { UserNames = { } };

        return new UsersToReturnMessage
        {
            UserNames = { users.UserNames }
        };
    }
}
