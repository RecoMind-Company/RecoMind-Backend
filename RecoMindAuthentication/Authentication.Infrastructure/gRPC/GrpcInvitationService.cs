using Authentication.Core.DTOs;
using Authentication.Core.Interfaces;
using Authentication.Infrastructure.gRPC.Protos;

namespace Authentication.Infrastructure.gRPC;

public class GrpcInvitationService(InvitationService.InvitationServiceClient invitationService) : IGrpcInvitationService
{
    public async Task<BaseToReturnDto> LoginAttempt(string email)
    {
        var request = new GetInvitationMessage { Email = email };
        var grpcResponse = await invitationService.LoginAttemptWithInvitationAsync(request);
        return new BaseToReturnDto { Success = grpcResponse.IsSuccess, Message = grpcResponse.Message };
    }
}
