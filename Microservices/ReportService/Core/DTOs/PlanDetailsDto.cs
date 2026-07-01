namespace Core.DTOs;

public class PlanDetailsDto
{
    public string Goal { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = [];
    public string Reasoning { get; set; } = string.Empty;
}
