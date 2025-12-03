using System.Text.Json.Serialization;

namespace Authentication.Core.DTOs;

public class AuthenticationDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime ExperiesOn { get; set; }
    public string Token { get; set; } = default!;
    public bool IsAuthenticated { get; set; }
    public List<string>? Roles { get; set; }
    public string? PhotoUrl { get; set; }
    public string? message { get; set; }
    [JsonIgnore]
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExp { get; set; }

}
