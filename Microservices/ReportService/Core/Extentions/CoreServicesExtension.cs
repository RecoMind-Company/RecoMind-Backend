using Core.Interfaces;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extentions;

public static class CoreServicesExtension
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IReportService, ReportService>();
    }
}
