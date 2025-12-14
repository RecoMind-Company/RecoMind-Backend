namespace Core.DTOs.AuthenticationDtos;

public class AuthResponseDto
{
    public string UserId { get; set; } = default!;
    public bool IsAuthenticated { get; set; }
    public string Message { get; set; } = default!;
}
