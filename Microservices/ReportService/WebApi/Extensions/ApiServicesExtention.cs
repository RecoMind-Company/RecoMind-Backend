namespace WebApi.Extensions;

public static class ApiServicesExtention
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }
}
