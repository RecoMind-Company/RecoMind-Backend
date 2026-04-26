
using Core.Interfaces;
using Core.Mapping;
using Core.Models;
using Core.Service;
using Core.Service.Interface;
using Infrastructure.Data;
using Infrastructure.Repository;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApi.Grpc;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddDbContext<SubscriptionDbContext>(options =>
                     options.UseSqlServer(
                         builder.Configuration.GetConnectionString("ProductionConnection_Subscription"),
                         sqlOptions =>
                         sqlOptions.MigrationsAssembly(typeof(SubscriptionDbContext).Assembly.FullName)
                         ));

            builder.WebHost.ConfigureKestrel(options =>
            {
                // اقرأ من environment أولاً (أولوية أعلى)
                var httpPort = int.Parse(
                    Environment.GetEnvironmentVariable("HTTP_PORT") ??
                    Environment.GetEnvironmentVariable("Kestrel_EndpointsHttp_Port") ??
                    builder.Configuration["Kestrel:Endpoints:Http:Port"] ??
                    "8001"
                );

                var grpcPort = int.Parse(
                    Environment.GetEnvironmentVariable("GRPC_PORT") ??
                    Environment.GetEnvironmentVariable("Kestrel_EndpointsGrpc_Port") ??
                    builder.Configuration["Kestrel:Endpoints:Grpc:Port"] ??
                    "5001"
                );

                options.ListenAnyIP(httpPort, o => o.Protocols = HttpProtocols.Http1);
                options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
            });

            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcExceptionHandler>();
            });            

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
                var jwt = builder.Configuration
                .GetSection("JwtOptions")
                .Get<JwtSettings>()
                ?? throw new Exception("JwtOptions are not configured");

                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(jwt.SecretKey)
                     ),

                    ClockSkew = TimeSpan.Zero, // ONLY FOR TESTING
                };
            });


            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            builder.Services.AddScoped(typeof(ISubscriptionCompanyService), typeof(SubscriptionCompanyService));
            builder.Services.AddScoped(typeof(ISubscriptionTypeService), typeof(SubscriptionTypeSevice));

            builder.Services.AddAutoMapper(typeof(SubscriptionCompanyMapping));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("OpenCors", policy =>
                {
                    policy
                        .AllowAnyOrigin()     // يسمح بأي دومين
                        .AllowAnyHeader()     // يسمح بأي هيدر
                        .AllowAnyMethod();    // يسمح بأي نوع HTTP Method
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();


            // app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseCors("OpenCors");

            app.MapGrpcService<SubscriptionGrpcService>();
            app.MapControllers();

            app.Run();
        }
    }
}
