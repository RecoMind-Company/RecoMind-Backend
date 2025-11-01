namespace Authentication.Core.Models;

public class VerificationCode
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Email { get; set; }
    public DateTime CreateAt { get; set; }
    public bool IsActive => DateTime.UtcNow < CreateAt.AddHours(1);
}
