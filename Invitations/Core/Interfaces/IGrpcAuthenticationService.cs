using Core.DTOs;

namespace Core.Interfaces;

public interface IGrpcAuthenticationService
{
    Task<AuthResponseDto> Register(string email, string role);
}
