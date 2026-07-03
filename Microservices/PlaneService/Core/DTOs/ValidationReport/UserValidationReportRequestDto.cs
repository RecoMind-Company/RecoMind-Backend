using System.Text.Json.Serialization;

namespace Core.DTOs.ValidationReport;

public class UserValidationReportRequestDto
{
    [JsonIgnore]
    public string? UserId { get; set; }
    [JsonIgnore]
    public string? CompanyId { get; set; }
    public string UserRequest { get; set; }
}
