using Core.DTOs;

namespace Core.Interfaces;

public interface IInvitationService
{
    Task<BaseToReturnDto> SendInvitationAsync(SendInvitationDto invitationDto);
    Task<InvitationDto> GetInvitation(string email);
    Task UpdateInvitationStatus(UpdateInvitationDto updateInvitationDto);

}
