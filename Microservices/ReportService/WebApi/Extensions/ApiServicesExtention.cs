namespace WebApi.Extensions;
using WebApi.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;


public static class ApiServicesExtention
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {


        builder.WebHost.ConfigureKestrel(options =>
            {
                // اقرأ من environment أولاً (أولوية أعلى)
                var httpPort = int.Parse(
                    Environment.GetEnvironmentVariable("HTTP_PORT") ??
                    Environment.GetEnvironmentVariable("Kestrel__Endpoints__Http__Port") ??
                    builder.Configuration["Kestrel:Endpoints:Http:Port"] ??
                    "8001"
                );

                var grpcPort = int.Parse(
                    Environment.GetEnvironmentVariable("GRPC_PORT") ??
                    Environment.GetEnvironmentVariable("Kestrel__Endpoints__Grpc__Port") ??
                    builder.Configuration["Kestrel:Endpoints:Grpc:Port"] ??
                    "5001"
                );

                options.ListenAnyIP(httpPort, o => o.Protocols = HttpProtocols.Http1);
                options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
            });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }



    
}
