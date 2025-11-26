namespace Core.DTOs;

public class AuthResponseDto
{
    public bool IsAuthenticated { get; set; }
    public string Message { get; set; } = default!;
}
