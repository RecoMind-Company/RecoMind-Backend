using Infrastructure.gRPC;
using Microsoft.OpenApi.Models;

namespace WebApi.Extensions;

using Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class ApiServicesExtention
{
    public static void AddPresentationServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {


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
            var jwt = configuration.GetSection("JwtOptions").Get<JwtSettings>()!;
            options.SaveToken = true;
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                ClockSkew = TimeSpan.Zero, // ONLY FOR TESTING
            };
        });
        builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration["Urls:TeamServiceUrl"]);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });

        var authorizationBuilder = builder.Services.AddAuthorizationBuilder();
        authorizationBuilder.AddPolicy("AllEmployees", p => p.RequireRole("admin", "manager", "teamleader", "employee"));
        authorizationBuilder.AddPolicy("Management", p => p.RequireRole("admin", "manager"));
        authorizationBuilder.AddPolicy("TeamLeadership", p => p.RequireRole("admin", "manager", "teamleader"));
    }




}
