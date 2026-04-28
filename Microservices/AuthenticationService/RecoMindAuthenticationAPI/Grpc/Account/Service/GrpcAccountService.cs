using Authentication.Core.DTOs;
using Authentication.Core.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Security.Claims;

namespace RecoMindAuthenticationAPI.Grpc.Account.Service;

public class GrpcAccountService(IAccountService accountService) : AccountService.AccountServiceBase
{
    public override async Task<BaseMessage> UpdateProfile(ProfileMessage request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var userEmail = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        IFormFile photoFile = null;
        if (request.PhotoContent.Any())
        {
            photoFile = new GrpcFormFile(request.PhotoContent.ToByteArray(), request.PhotoFileName);
        }
        var ProfileDto = new ProfileDto
        {
            Email = request.Email,
            Name = request.Name,
            Phone = request.Phone,
            Photo = photoFile
        };
        var response = await accountService.EditProfile(ProfileDto, userEmail);
        if (!response.Success)
            throw new RpcException(new Status(StatusCode.InvalidArgument, response.Message));
        return new BaseMessage
        {
            IsSuccess = response.Success,
            Message = response.Message
        };

    }

    public override async Task<ProfileToReturnMessage> GetProfile(Empty request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var userEmail = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var response = await accountService.GetProfile(userEmail);
        if (response is null)
            throw new RpcException(new Status(StatusCode.NotFound, "this user is not found"));

        return new ProfileToReturnMessage
        {
            Name = response.Name,
            Email = response.Email,
            Phone = response.Phone ?? string.Empty,
            Photo = response.Photo ?? string.Empty
        };
    }

    public override async Task<BaseMessage> DeleteUser(DeleteMessage request, ServerCallContext context)
    {
        var response = await accountService.DeleteUser(request.Id);
        if (!response.Success)
            throw new RpcException(new Status(StatusCode.InvalidArgument, response.Message));
        return new BaseMessage
        {
            IsSuccess = response.Success,
            Message = response.Message
        };
    }

    public override async Task<GetJobTitlesListResponse> GetJobTitlesList(GetJobTitlesListRequest request, ServerCallContext context)
    {
        var userIds = request.Ids;

        var jobTitles = await accountService.GetUsersJobTitle(userIds);

        var response = new GetJobTitlesListResponse();

        response.JobTitlesList.AddRange(jobTitles.Select(x => new JobTitlesList
        {
            UserId = x.UserId,
            JobTitle = x.JobTitle
        }));

        return response;
    }
}

