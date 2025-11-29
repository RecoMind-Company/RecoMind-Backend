using Core.DTOs;

namespace Core.Interfaces;

public interface IInvitationService
{
    Task<BaseToReturnDto> SendInvitationAsync(SendInvitationDto invitationDto);
    Task<BaseToReturnDto> LoginAttemptWithInvitation(string email);

}
