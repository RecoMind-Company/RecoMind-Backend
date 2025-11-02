namespace Authentication.Core.DTOs;

public class ResetPasswordDto : BasePasswordDto
{
    public string OldPassword { get; set; }
}
