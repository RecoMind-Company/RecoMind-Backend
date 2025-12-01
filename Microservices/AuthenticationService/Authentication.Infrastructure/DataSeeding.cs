using Authentication.Core.Constants;
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
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            await roleManager.CreateAsync(new IdentityRole(Roles.Employee));
            await roleManager.CreateAsync(new IdentityRole(Roles.TeamLeader));
        }

    }
}
