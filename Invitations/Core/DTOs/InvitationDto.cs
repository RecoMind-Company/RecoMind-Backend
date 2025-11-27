namespace Core.DTOs;

public class InvitationDto : UpdateInvitationDto
{
    public string Email { get; set; } = default!;
    public bool IsActive { get; set; }
}
