using System.ComponentModel.DataAnnotations;

namespace Authentication.Core.DTOs;

public class ForgetPasswordDto
{
    [EmailAddress]
    public string Email { get; set; }
}
