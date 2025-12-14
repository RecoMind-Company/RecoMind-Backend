using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Infrastructure.Context;
using Authentication.Infrastructure.gRPC;
using Authentication.Infrastructure.gRPC.CompanyGrpc;
using Authentication.Infrastructure.gRPC.TeamGrpc;
using Authentication.Infrastructure.Repository;
using Authentication.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Infrastructure.Extensions;

public static class InfrastructureServicesExtension
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthenticationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("ProdcutionConnectionString_Auth"));
        });
        services.AddIdentityCore<AppUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<AuthenticationDbContext>();
        services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<DataSeeding>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IGrpcInvitationService, GrpcInvitationService>();
        services.AddScoped<IGrpcTeamService, GrpcTeamService>();
        services.AddScoped<IGrpcCompanyService, GrpcCompanyServiceImp>();
    }
}
