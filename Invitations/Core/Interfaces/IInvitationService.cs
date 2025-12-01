using Core.DTOs;

namespace Core.Interfaces;

public interface IInvitationService
{
    Task<BaseToReturnDto> SendInvitationAsync(SendInvitationDto invitationDto);
    Task<BaseToReturnDto> LoginAttemptWithInvitation(string email);
    Task<InvitationsToReturnDto> GetInvitationByIdAsync(int id);
    Task<IEnumerable<InvitationsToReturnDto>> GetInvitationsByStatus(GetInvitationDto getInvitationDto);
    Task CheckAndExpireInvitations();

}
