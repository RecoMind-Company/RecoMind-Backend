using Core.Interfaces;
using Infrastructure.AI;
using Infrastructure.Context;
using Infrastructure.FileStorage;
using Infrastructure.gRPC;
using Infrastructure.Repository;
using MassTransit;
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
        //services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
        //services.AddHangfireServer();
        services.AddHttpClient<IGenerateReportService, GenerateReportService>(client =>
        {
            client.BaseAddress = new Uri("https://ai.recomind.site/reporting/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddTransientHttpErrorPolicy(policy =>
            policy.WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        );
        services.AddHttpClient<IDataAssignService, DataAssignService>(client =>
        {
            client.BaseAddress = new Uri("https://ai.recomind.site/embedding/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddTransientHttpErrorPolicy(policy =>
            policy.WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        );
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IGrpcTeamService, GrpcTeamService>();

        services.AddMassTransit(x =>
         {
             x.UsingRabbitMq((context, cfg) =>
             {
                 var rabbitSettings = configuration.GetSection("RabbitMQ");

                 cfg.Host(rabbitSettings["Host"] ?? "localhost",
                     ushort.TryParse(rabbitSettings["Port"], out var port) ? port : (ushort)5672,
                     rabbitSettings["VirtualHost"] ?? "/",
                     h =>
                     {
                         h.Username(rabbitSettings["Username"] ?? "recomind");
                         h.Password(rabbitSettings["Password"] ?? "recomind");
                     });
             });
         });

    }
}
