
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

                options.ListenAnyIP(7094, listenOptions =>
                {
                    listenOptions.UseHttps();
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
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
