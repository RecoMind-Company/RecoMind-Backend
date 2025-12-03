namespace Authentication.Core.DTOs;

public class UserToReturnDto
{
    public string Id { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Email { get; set; } = default!;
}
