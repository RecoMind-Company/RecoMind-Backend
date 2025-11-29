using Core.DTOs;
using Core.Interfaces;
using Core.Models;

namespace Core.Services;

public class InvitationService(IGrpcAuthenticationService grpcAuthenticationService,
                               IInvitationRepository repository,
                               IUnitOfWork unitOfWork) : IInvitationService
{

    public async Task<BaseToReturnDto> SendInvitationAsync(SendInvitationDto invitationDto)
    {
        var invitation = new Invitation
        {
            SenderId = invitationDto.SenderId,
            Email = invitationDto.Email,
            ReceiverRole = invitationDto.ReciverRole,
            CreatedAt = DateTime.UtcNow,
            CompanyId = string.Empty // *** THIS IS FOR FUTURE USE, SETTING IT TO EMPTY STRING FOR NOW ***
        };
        await repository.CreateAsync(invitation);
        await unitOfWork.Save();

        var grpcResponse = await grpcAuthenticationService.Register(invitationDto.Email, invitationDto.ReciverRole);
        if (!grpcResponse.IsAuthenticated)
        {
            return new BaseToReturnDto
            {
                Message = $"Failed to send invitation: {grpcResponse.Message}"
            };
        }
        // Check if the gRPC authentication was successful

        return new BaseToReturnDto
        {
            IsSuccess = true,
            Message = "Invitation sent successfully."
        };
    }
    public async Task<BaseToReturnDto> LoginAttemptWithInvitation(string email)
    {
        var invitation = await repository.Find(i => i.Email == email);
        if (invitation is null)
            return new BaseToReturnDto { IsSuccess = false, Message = "There is no invitation for this email" };
        var status = invitation.TryToAcceptInvitation();
        if (status == Status.Expired)
        {
            repository.Update(invitation);
            await unitOfWork.Save();
            return new BaseToReturnDto { IsSuccess = false, Message = "The invitation has expired." };
        }
        else if (status == Status.Accepted)
        {
            repository.Update(invitation);
            await unitOfWork.Save();
            return new BaseToReturnDto { IsSuccess = true, Message = "Invitation accepted successfully." };
        }
        return new BaseToReturnDto { IsSuccess = true, Message = "Invitation is allready accepted." };
    }
}
