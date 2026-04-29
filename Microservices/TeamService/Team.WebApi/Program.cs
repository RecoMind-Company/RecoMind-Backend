using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Team.Core.Interfaces;
using Team.Core.Mapper;
using Team.Infrastructure.Data;
using Team.Infrastructure.gRPC;
using Team.Infrastructure.Repositories;
using Team.WebApi.GrpcServices;
namespace Team.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            builder.Services.AddDbContext<TeamDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection_Team"),// DefaultConnection"), //

                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)
                )
            );

            builder.Configuration.AddEnvironmentVariables();

            // Configure CORS Policy
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

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddAutoMapper(typeof(TeamProfile));
            builder.Services.AddScoped<ITeamRepository, TeamRepository>();
            builder.Services.AddScoped<ITeamService, Core.Services.TeamService>();
            builder.Services.AddScoped<IAuthGrpcService, AuthGrpcService>();

            builder.Services.AddGrpc();
            builder.Services.AddGrpcReflection();
            // 2. تسجيل الـ gRPC Client (ده الأهم)
            builder.Services.AddGrpcClient<AccountService.AccountServiceClient>(options =>
            {
                var url = builder.Configuration["Urls:AuthServiceUrl"];
                options.Address = new Uri(url!);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });


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

            var authorizationBuilder = builder.Services.AddAuthorizationBuilder();
            authorizationBuilder.AddPolicy("AllEmployees", p => p.RequireRole("admin", "manager", "teamleader", "employee"));
            authorizationBuilder.AddPolicy("Leadership", p => p.RequireRole("admin", "manager", "teamleader"));
            authorizationBuilder.AddPolicy("Management", p => p.RequireRole("admin", "manager"));

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("OpenCors");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<TeamGrpcServiceImpl>();
            app.MapControllers();

            app.Run();
        }
    }
}
