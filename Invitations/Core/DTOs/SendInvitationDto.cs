using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class SendInvitationDto
{
    public string SenderId { get; set; } = default!;
    [EmailAddress]
    [Required]
    public string Email { get; set; } = default!;
    [Required]
    public string ReciverRole { get; set; } = default!;

}
