using Core.DTOs;
using Core.Interfaces;
using Core.Models;

namespace Core.Services;

public class InvitationService(IGrpcAuthenticationService grpcAuthenticationService,
                               IInvitationRepository repository,
                               IUnitOfWork unitOfWork) : IInvitationService
{
    public async Task<BaseToReturnDto> SendInvitationAsync(InvitationDto invitationDto)
    {
        var grpcResponse = await grpcAuthenticationService.Register(invitationDto.Email, invitationDto.ReciverRole);
        if (!grpcResponse.IsAuthenticated)
        {
            return new BaseToReturnDto
            {
                Message = $"Failed to send invitation: {grpcResponse.Message}"
            };
        }
        // Check if the gRPC authentication was successful
        var invitation = new Invitation
        {
            SenderId = invitationDto.SenderId,
            email = invitationDto.Email,
            ReceiverRole = invitationDto.ReciverRole,
            Status = Status.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await repository.CreateAsync(invitation);
        await unitOfWork.Save();
        return new BaseToReturnDto
        {
            IsSuccess = true,
            Message = "Invitation sent successfully."
        };
    }
}
