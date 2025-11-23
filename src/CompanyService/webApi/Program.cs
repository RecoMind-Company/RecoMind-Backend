
using Core.Configuration;
using Core.Interfaces;
using Core.Service;
using Core.Service.Interface;
using Infrastructure.Data;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using webApi.Grpc.GrpcImplementations;
using webApi.Mapping;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Campany.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            builder.Services.AddDbContext<CompanyDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions =>
                        sqlOptions.MigrationsAssembly(typeof(CompanyDbContext).Assembly.FullName)                                                  
                        ));

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            builder.Services.AddScoped(typeof(ICompanyService), typeof(Core.Service.CompanyService));

            builder.Services.AddAutoMapper( typeof(CopmanyMapping),typeof(MappingForRpc));
               
            builder.Services.AddLogging();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add gRPC services to the container. 
            builder.Services.AddGrpc();

            // Configure Kestrel to listen on a specific port for gRPC
            builder.WebHost.ConfigureKestrel(options =>
            {

                options.ListenAnyIP(5001, listenOptions =>
                {
                    listenOptions.UseHttps(); 
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
            });     

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddGrpcReflection();
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
                       
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapGrpcService<CompanyServiceImpl>();
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            app.Run();
        }
    }
}
