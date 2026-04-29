using DatabaseSetting.Core.Interfaces;
using DatabaseSetting.Core.Mapper;
using DatabaseSetting.Core.Services;
using DatabaseSetting.Infrastructure.Data;
using DatabaseSetting.Infrastructure.Repositories;
using DatabaseSetting.WebApi.GrpcServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
                    builder.Configuration.GetConnectionString("ProdcutionConnection_DbSetting"), // "DefaultConnection"),//
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                )
            );

            builder.Services.AddControllers();
            builder.Services.AddScoped<IDbSettingRepository, DbSettingRepository>();
            builder.Services.AddScoped<IDbSettingService, DbSettingService>();
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddGrpc();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(cfg =>
            {
                cfg.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                });
                cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BearerAuth" }
                        },
                        []
                    }
                });
            });

            builder.Services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
                    ValidAudience = builder.Configuration["JwtOptions:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:SecretKey"])),
                    ClockSkew = TimeSpan.Zero,
                };
            });

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    var httpPort = int.Parse(
            //        Environment.GetEnvironmentVariable("HTTP_PORT") ??
            //        Environment.GetEnvironmentVariable("Kestrel__Endpoints__Http__Port") ??
            //        builder.Configuration["Kestrel:Endpoints:Http:Port"] ??
            //        "8001"
            //    );

            //    var grpcPort = int.Parse(
            //        Environment.GetEnvironmentVariable("GRPC_PORT") ??
            //        Environment.GetEnvironmentVariable("Kestrel__Endpoints__Grpc__Port") ??
            //        builder.Configuration["Kestrel:Endpoints:Grpc:Port"] ??
            //        "5001"
            //    );

            //    options.ListenAnyIP(httpPort, o => o.Protocols = HttpProtocols.Http1);
            //    options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
            //});

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("OpenCors", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var authorizationBuilder = builder.Services.AddAuthorizationBuilder();
            authorizationBuilder.AddPolicy("ManagerRole", p => p.RequireRole("admin", "manager"));

            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("OpenCors");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<DbSettingGrpcServiceImpl>();
            app.MapControllers();

            app.Run();
        }
    }
}
