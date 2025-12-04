using Infrastructure.gRPC.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Builder;

namespace WebApi.Extensions;

public static class ApiServiceExtention
{
    public static void AddPresentationServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddControllers();
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
                ValidIssuer = configuration["JwtOptions:Issuer"],
                ValidAudience = configuration["JwtOptions:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtOptions:SecretKey"])),
                ClockSkew = TimeSpan.Zero, // ONLY FOR TESTING
            };
        });

        builder.WebHost.ConfigureKestrel(options =>
            {
                var httpPort = int.Parse(builder.Configuration["Kestrel:Endpoints:Http:Port"] ?? "8001");
                var grpcPort = int.Parse(builder.Configuration["Kestrel:Endpoints:Grpc:Port"] ?? "5001");

                options.ListenAnyIP(httpPort, o => o.Protocols = HttpProtocols.Http1);
                options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
            });

        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        builder.Services.AddGrpcClient<AuthenticationService.AuthenticationServiceClient>(o =>
        {
            o.Address = new Uri(configuration["Urls:AuthenticationServiceUrl"]);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            
            
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            
            return new HttpClientHandler();
        });
    }
}
