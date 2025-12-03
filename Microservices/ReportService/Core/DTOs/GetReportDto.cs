using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class GetReportDto
{
    [Required]
    public string UserRequest { get; set; } = default!;
}
