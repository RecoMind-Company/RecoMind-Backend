
using Core.Extentions.ExceptionHandeler;
using Core.Interfaces;
using Core.Mapping;
using Core.Services;
using Core.Services.Interface;
using Infrastructure.Data;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using WebApi.Grpc;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // DbContext
            builder.Services.AddDbContext<PlanServiceDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("ProdcutionConnection_PlanService"),
                        sqlOptions =>
                        sqlOptions.MigrationsAssembly(typeof(PlanServiceDbContext).Assembly.FullName)
                        ));

            //  Configurations for gRPC

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

            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<GlobalGrpcExceptionInterceptor>();
            });
            
            builder.Services.AddSingleton<GlobalGrpcExceptionInterceptor>();
            
                                    // Controllers
            builder.Services.AddControllers();

                                    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

                                    // Dependency Injection
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddAutoMapper(typeof(PlanMapping));
            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            var app = builder.Build();

                                    // Configure the HTTP request pipeline.

            
            
                app.UseSwagger();
                app.UseSwaggerUI();
            

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

                

            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<PlanServiceImpl>();

            app.Run();
        }
    }
}
