using Core.Interfaces;
using Hangfire;
using Infrastructure.AI;
using Infrastructure.Context;
using Infrastructure.FileStorage;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Infrastructure.Extentions;

public static class InfrastructureServicesExtension
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductionConnection_Report");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
        services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
        services.AddHangfireServer();
        services.AddHttpClient<IGenerateReportService, GenerateReportService>(client =>
        {
            client.BaseAddress = new Uri("https://ai.recomind.site/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddTransientHttpErrorPolicy(policy =>
            policy.WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        );
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGenerateReportService, GenerateReportService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
    }
}
