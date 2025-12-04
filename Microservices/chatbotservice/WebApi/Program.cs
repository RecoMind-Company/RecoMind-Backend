using Core.Interfaces;
using Core.Mapping;
using Core.Models;
using Core.Services;
using Core.Services.Interface;
using Infrastructure.Data;
using Infrastructure.UnitOfWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<ChatbotDbContext>(options =>
                   options.UseSqlServer(
                       builder.Configuration.GetConnectionString("ProdcutionConnection_Chatbot"),
                       sqlOptions =>
                       sqlOptions.MigrationsAssembly(typeof(ChatbotDbContext).Assembly.FullName)
                       ));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Auto-register all AutoMapper profiles in loaded assemblies (no need to update this file when profiles are added)
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            builder.Services.AddScoped(typeof(IChatBotService), typeof(ChatBotService));
            builder.Services.AddScoped(typeof(IAiClientService), typeof(AiClientService));


            var app = builder.Build();

            // Validate AutoMapper configuration at startup (fail fast)
            using (var scope = app.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
            }

            // Configure the HTTP request pipeline.
                app.UseSwagger();
                app.UseSwaggerUI();
            

            // app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}