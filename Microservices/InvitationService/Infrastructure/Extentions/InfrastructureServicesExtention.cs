using Core.Interfaces;
using Hangfire;
using Infrastructure.Context;
using Infrastructure.gRPC;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure.Extentions;

public static class InfrastructureServicesExtention
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductionConnectionString_Invitation");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
        services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
        services.AddHangfireServer();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGrpcAuthenticationService, GrpcAuthenticationService>();

    }
}
