namespace Authentication.Core.Models;

public class CodeVerification
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Email { get; set; }
    public DateTime CreateAt { get; set; }
}
