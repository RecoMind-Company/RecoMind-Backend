using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core.DTOs;

public class SendInvitationDto
{
    [JsonIgnore]
    public string? SenderId { get; set; } = default!;
    [EmailAddress]
    [Required]
    public string Email { get; set; } = default!;
    [Required]
    public string ReciverRole { get; set; } = default!;
    [Required]
    public string CompanyId { get; set; } = default!;
}
