using Core.DTOs.AuthenticationDtos;

namespace Core.Interfaces;

public interface IGrpcAuthenticationService
{
    Task<AuthResponseDto> Register(string email, string role);
}
