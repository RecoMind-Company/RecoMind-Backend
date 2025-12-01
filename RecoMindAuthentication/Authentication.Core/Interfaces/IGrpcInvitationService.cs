using Authentication.Core.DTOs;

namespace Authentication.Core.Interfaces;

public interface IGrpcInvitationService
{
    Task<BaseToReturnDto> LoginAttempt(string email);
}
