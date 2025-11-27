using Authentication.Core.DTOs.InvitationDtos;
using Authentication.Core.Interfaces;
using Authentication.Infrastructure.gRPC.Protos;

namespace Authentication.Infrastructure.gRPC;

public class GrpcInvitationService(InvitationService.InvitationServiceClient invitationService) : IGrpcInvitationService
{
    public async Task<InvitationDto> GetInvitation(string email)
    {
        var request = new GetInvitationMessage { Email = email };
        var response = await invitationService.GetInvitationAsync(request);
        return new InvitationDto
        {
            Id = response.Id,
            Email = response.Email,
            Status = response.Status.ToString(),
            IsActive = response.IsActive
        };
    }

    public async Task UpdateInvitation(InvitationDto invitation)
    {
        var request = new InvitationMessage
        {
            Id = invitation.Id,
            Email = invitation.Email,
            IsActive = invitation.IsActive,
            Status = Enum.Parse<InvitationStatus>(invitation.Status)
        };
        await invitationService.UpdateInvitationAsync(request);
    }
}
