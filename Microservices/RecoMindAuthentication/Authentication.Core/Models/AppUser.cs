using Microsoft.AspNetCore.Identity;

namespace Authentication.Core.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; }

}
