using Core.Interfaces;
using Grpc.Core;
using WebApi.gRPC.Protos;

namespace WebApi.gRPC;

public class GrpcInvitationService(IInvitationService invitationService) : InvitationService.InvitationServiceBase
{
    public override async Task<BaseToReturnMessage> LoginAttemptWithInvitation(GetInvitationMessage request, ServerCallContext context)
    {
        // Call the method form the invitation service
        // map the response to gRPC response and return it
        var response = await invitationService.LoginAttemptWithInvitation(request.Email);
        return new BaseToReturnMessage { IsSuccess = response.IsSuccess, Message = response.Message };
    }
}
