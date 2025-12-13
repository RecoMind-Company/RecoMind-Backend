namespace Authentication.Core.Models;

public class UsersJobTitle
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string JobTitle { get; set; } = default!;
    public AppUser? User { get; set; }
}
