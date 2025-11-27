using Core.DTOs;
using Core.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using WebApi.gRPC.Protos;
using Enum = System.Enum;

namespace WebApi.gRPC;

public class GrpcInvitationService(IInvitationService invitationService) : InvitationService.InvitationServiceBase
{
    public override async Task<InvitationMessage> GetInvitation(GetInvitationMessage request, ServerCallContext context)
    {
        InvitationMessage response = new InvitationMessage();
        var result = await invitationService.GetInvitation(request.Email);
        if (result is null)
        {
            response.Id = 0;
            response.Email = string.Empty;
            response.IsActive = false;
            response.Status = InvitationStatus.Unknown;
            return response;
        }
        response.Id = result.Id;
        response.Email = result.Email;
        response.IsActive = result.IsActive;
        response.Status = Enum.Parse<InvitationStatus>(result.Status, ignoreCase: true);
        return response;
    }
    public override async Task<Empty> UpdateInvitation(InvitationMessage request, ServerCallContext context)
    {
        // map to Dto
        // call the Update service
        var updateDto = new UpdateInvitationDto { Id = request.Id, Status = request.Status.ToString() };
        await invitationService.UpdateInvitationStatus(updateDto);
        return new Empty();
    }
}
