namespace Core.DTOs;

public class PlanDetailsDto
{
    public string Goal { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = [];
    public string? Reasoning { get; set; }
    public string? Scenarios { get; set; }
    public string? RiskManagement { get; set; }
}
