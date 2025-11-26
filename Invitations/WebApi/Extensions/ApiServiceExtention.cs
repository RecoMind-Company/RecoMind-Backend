using Infrastructure.gRPC.Protos;

namespace WebApi.Extensions;

public static class ApiServiceExtention
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddGrpcClient<AuthenticationService.AuthenticationServiceClient>(o =>
        {
            o.Address = new Uri("https://localhost:7264");
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
