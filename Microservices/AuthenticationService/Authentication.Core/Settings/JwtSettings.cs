namespace Authentication.Core.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public double DurationInHours { get; set; }
}
