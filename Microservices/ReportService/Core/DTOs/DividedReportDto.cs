namespace Core.DTOs;

public class DividedReportDto
{
    public string Id { get; set; } = default!;
    public string TeamId { get; set; } = default!;
    public PlanDetailsDto ShortTerm { get; set; } = new();
    public PlanDetailsDto MidTerm { get; set; } = new();
    public PlanDetailsDto LongTerm { get; set; } = new();
    public DateTime GeneratedDate { get; set; }

    // if getting report have any errors
    public string? ErrorMessage { get; set; }
}
