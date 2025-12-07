namespace Authentication.Core.DTOs;

public class ProfileToReturnDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Photo { get; set; }
}
