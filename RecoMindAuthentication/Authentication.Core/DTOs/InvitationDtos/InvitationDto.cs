namespace Authentication.Core.DTOs.InvitationDtos;

public class InvitationDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public bool IsActive { get; set; }
}
