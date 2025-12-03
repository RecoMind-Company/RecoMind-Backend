namespace Core.DTOs;

public class AiReportResponseDto
{
    public string AiResponse { get; set; } = default!;
    public DateTime GeneratedDate { get; set; }
}
