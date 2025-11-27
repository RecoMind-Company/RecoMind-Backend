using Infrastructure.gRPC.Protos;

namespace WebApi.Extensions;

public static class ApiServiceExtention
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        builder.Services.AddGrpcClient<AuthenticationService.AuthenticationServiceClient>(o =>
        {
            o.Address = new Uri(configuration["Urls:AuthenticationServiceUrl"]);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            if (builder.Environment.IsDevelopment())
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            }
            return new HttpClientHandler();
        });
    }
}
