namespace Core.Settings;

public class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int DurationInHours { get; set; }
    public string SecretKey { get; set; } = default!;
}
