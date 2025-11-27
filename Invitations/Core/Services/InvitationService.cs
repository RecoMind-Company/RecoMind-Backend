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
            Email = invitationDto.Email,
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
    public async Task<InvitationDto> GetInvitation(string email)
    {
        var invitation = await repository.Find(i => i.Email == email);
        if (invitation is null)
            return null;
        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Status = invitation.Status.ToString(),
            IsActive = invitation.IsActive
        };
    }

    public async Task UpdateInvitationStatus(UpdateInvitationDto updateInvitationDto)
    {
        var invitation = await repository.GetByIdAsync(updateInvitationDto.Id);

        // The status will be a enum in gRPC for better type safety
        invitation.Status = Enum.Parse<Status>(updateInvitationDto.Status, ignoreCase: true);
        repository.Update(invitation);
        await unitOfWork.Save();
    }
}
