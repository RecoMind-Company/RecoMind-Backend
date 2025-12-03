using Core.Interfaces;
using Core.MappingProfiles;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
namespace Core.Extensions;

public static class CoreServicesExtention
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddAutoMapper(cfg => { }, typeof(InvitationProfile).Assembly);
    }
}
