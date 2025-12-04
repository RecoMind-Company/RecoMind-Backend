using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class GetReportDto
{
    [Required]
    public string UserRequest { get; set; } = default!;
    [Required]
    public string TeamId { get; set; } = default!;
    [Required]
    public string Periodic { get; set; } = default!;
}
