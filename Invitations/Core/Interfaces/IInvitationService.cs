using Core.DTOs;

namespace Core.Interfaces;

public interface IInvitationService
{
    Task<BaseToReturnDto> SendInvitationAsync(InvitationDto invitationDto);
}
