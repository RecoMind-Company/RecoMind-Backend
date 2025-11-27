using Authentication.Core.DTOs.InvitationDtos;

namespace Authentication.Core.Interfaces;

public interface IGrpcInvitationService
{
    Task<InvitationDto> GetInvitation(string email);
    Task UpdateInvitation(InvitationDto invitation);
}
