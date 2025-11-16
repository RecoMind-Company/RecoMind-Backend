using DatabaseSetting.Core.Interfaces;
using DatabaseSetting.Core.Services;
using DatabaseSetting.Infrastructure.Data;
using DatabaseSetting.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace DatabaseSetting.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database Connection String
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("ProdcutionConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                )
            );


            // Dependency Injection for Repositories and Services
            builder.Services.AddScoped<IDbSettingRepository, DbSettingRepository>();
            builder.Services.AddScoped<IDbSettingService, DbSettingService>();
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            app.Run();
        }
    }
}
