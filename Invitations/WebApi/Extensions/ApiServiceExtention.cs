namespace WebApi.Extensions;

public static class ApiServiceExtention
{
    public static void AddPresentationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}
