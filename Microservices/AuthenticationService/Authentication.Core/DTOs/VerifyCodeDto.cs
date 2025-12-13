using System.ComponentModel.DataAnnotations;

namespace Authentication.Core.DTOs;

public class VerifyCodeDto
{
    [EmailAddress]
    public string Email { get; set; }
    public string Code { get; set; }
}
