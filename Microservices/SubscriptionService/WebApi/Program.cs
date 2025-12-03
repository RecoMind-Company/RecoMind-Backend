
using Core.Interfaces;
using Core.Mapping;
using Core.Models;
using Core.Service;
using Core.Service.Interface;
using Infrastructure.Data;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WebApi.Grpc;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<SubscriptionDbContext>(options =>
                     options.UseSqlServer(
                         builder.Configuration.GetConnectionString("ProdcutionConnection"),
                         sqlOptions =>
                         sqlOptions.MigrationsAssembly(typeof(SubscriptionDbContext).Assembly.FullName)
                         ));

            builder.WebHost.ConfigureKestrel(options =>
            {

                options.ListenAnyIP(7142, listenOptions =>
                {
                    listenOptions.UseHttps();
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
            });

            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcExceptionHandler>();
            });


            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            builder.Services.AddScoped(typeof(ISubscriptionService), typeof(SubscriptionService));

            builder.Services.AddAutoMapper(typeof(SubscriptionMapping));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGrpcService<SubscriptionGrpcService>();
            app.MapControllers();

            app.Run();
        }
    }
}
