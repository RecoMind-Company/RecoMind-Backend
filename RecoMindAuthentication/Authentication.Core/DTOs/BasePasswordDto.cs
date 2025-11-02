using System.ComponentModel.DataAnnotations;

namespace Authentication.Core.DTOs;

public class BasePasswordDto
{
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "the password and the confirm password are not the same")]
    public string ConfirmNewPassword { get; set; }
}
