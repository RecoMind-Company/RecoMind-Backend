using Authentication.Core.Models;
using Authentication.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Infrastructure;


public class DataSeeding(AuthenticationDbContext dbContext,
                         UserManager<AppUser> userManager,
                         RoleManager<IdentityRole> roleManager)
{
    public async Task DataSeedAsync()
    {
        if (!roleManager.Roles.Any())
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Manager"));
            await roleManager.CreateAsync(new IdentityRole("TeamLeader"));
        }

    }
}
