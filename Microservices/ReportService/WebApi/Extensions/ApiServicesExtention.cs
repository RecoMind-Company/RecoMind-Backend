using Infrastructure.gRPC;

namespace WebApi.Extensions;

public static class ApiServicesExtention
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration["Urls:TeamServiceUrl"]);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });
    }
}
