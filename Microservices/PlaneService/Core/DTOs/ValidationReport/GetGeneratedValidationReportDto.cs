using System.Text.Json.Serialization;

namespace Core.DTOs.ValidationReport;

public class GetGeneratedValidationReportDto
{
    public string TaskId { get; set; }
    [JsonIgnore]
    public string? CompanyId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
