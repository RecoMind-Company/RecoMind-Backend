using Core.Interfaces;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
namespace Core.Extensions;

public static class CoreServicesExtention
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IInvitationService, InvitationService>();
    }
}
