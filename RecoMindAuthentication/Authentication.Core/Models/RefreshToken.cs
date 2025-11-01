using Microsoft.EntityFrameworkCore;

namespace Authentication.Core.Models;
[Owned]
public class RefreshToken
{
    public string Token { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? RevokeOn { get; set; }
    public bool IsExpiered => DateTime.UtcNow > ExpiresOn;
    public bool IsActive => RevokeOn == null && !IsExpiered;
}
