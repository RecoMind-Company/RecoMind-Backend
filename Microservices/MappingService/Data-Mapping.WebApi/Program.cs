using Data_Mapping.Core.Interfaces;
using Data_Mapping.Core.Mapper;
using Data_Mapping.Core.Services;
using Data_Mapping.Infrastructure.Data;
using Data_Mapping.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Data_Mapping.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Database Configuration
            builder.Services.AddDbContext<MappingDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })
            );

            // 2. CORS Policy
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

            // 3. Swagger with JWT Support
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
                    Description = "Enter 'Bearer' [space] and then your valid token."
                });
                cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BearerAuth" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 4. Authentication & JWT
            builder.Services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

            // 5. Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Management", policy => policy.RequireRole("admin", "manager"));
                options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
            });

            // 6. Dependency Injection (DI) & Automapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddScoped<IMappingRepository, MappingRepository>();
            builder.Services.AddScoped<IMappingService, MappingService>();
            builder.Services.AddControllers();

            //7.Kestrel Configuration
            builder.WebHost.ConfigureKestrel(options =>
            {
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


            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("OpenCors");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}