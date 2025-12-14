namespace Core.DTOs;

public class ReportDto
{
    public string? Id { get; set; }
    public string? TeamId { get; set; }
    public string? UserId { get; set; }
    public string? Periodic { get; set; }
    public string? Content { get; set; }
    public DateTime GeneratedDate { get; set; }
}
