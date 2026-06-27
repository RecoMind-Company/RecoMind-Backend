using Core.Interfaces;
using Core.Mapping;
using Core.Models;
using Core.Service;
using Core.Service.Interface;
using Core.Service.Interface.AI;
using GrpcClients.Quest;
using GrpcClients.Team;
using Hangfire;
using Infrastructure.AI;
using Infrastructure.BackgroundJobs;
using Infrastructure.Data;
using Infrastructure.GrpcClients.Quest;
using Infrastructure.GrpcClients.Team;
using Infrastructure.Messaging;
using Infrastructure.UnitOfWork;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using System;
using System.Text;
using webApi.Grpc;
using WebApi.GrpcServer;

namespace webApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            //
            //
            // -------------------------------  NOTE THAT I AM USING THE DEFAULT CONNECTION STRING NOT PRODUCTION ---------------------------------
            //
            //
            //

            builder.Services.AddDbContext<PlanDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection_Plan"));
            });
            //Setup Hangfire with SQL Server storage
            builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("ProductionConnection_Plan")));
            builder.Services.AddHangfireServer();
            builder.Services.AddScoped<IBackgroundService, HangfireSetUpJobs>();
            builder.Services.AddGrpc();

            builder.Services.AddScoped<PlanServiceImpl>();
            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UOW<>));
            builder.Services.AddScoped(typeof(IPlanService), typeof(Core.Service.PlanService));
            builder.Services.AddScoped(typeof(IPlanType), typeof(Core.Service.PlanType));
            builder.Services.AddScoped(typeof(IStatus), typeof(Core.Service.Status));
            builder.Services.AddScoped<ITeamGrpcClient, TeamGrpcClientImpl>();
            builder.Services.AddScoped<IQuestGrpcClient, QuestGrpcClientImplementation>();
            builder.Services.AddScoped<IPlanEventPublisher, PlanEventPublisher>();
            builder.Services.AddScoped<IModuleService, ModuleService>();

            //AI plan generation Service
            var AiServiceUrl = builder.Configuration.GetValue<string>("AI:AIPlanGeneratorUrl");
            var aiApiKey = builder.Configuration.GetValue<string>("AI:ApiKey");
            builder.Services.AddHttpClient<IPlanGeneratorService, PlanGeneratorService>(client =>
            {
                client.BaseAddress = new Uri(AiServiceUrl!);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("X-API-Key", aiApiKey);
                client.Timeout = TimeSpan.FromSeconds(60);

            }).AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(3,
               retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            );

            builder.Services.AddAutoMapper(typeof(PlanMapper));
            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(options =>
            {
                options.Address = new Uri(builder.Configuration.GetValue<string>("GrpcSettings:TeamServiceUrl"));
            });

            builder.Services.AddGrpcClient<GrpcQuestsService.GrpcQuestsServiceClient>(options =>
            {
                options.Address = new Uri(builder.Configuration.GetValue<string>("GrpcSettings:TaskServiceUrl"));
            });

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitSettings = builder.Configuration.GetSection("RabbitMQ");

                    cfg.Host(rabbitSettings["Host"] ?? "localhost",
                        ushort.TryParse(rabbitSettings["Port"], out var port) ? port : (ushort)5672,
                        rabbitSettings["VirtualHost"] ?? "/",
                        h =>
                        {
                            h.Username(rabbitSettings["Username"] ?? "recomind");
                            h.Password(rabbitSettings["Password"] ?? "recomind");
                        });
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlanService", Version = "v1" });
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddGrpcReflection();
            }

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


            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("OpenCors");

            app.MapControllers();

            app.MapGrpcService<PlanServiceImpl>();
            app.MapGrpcService<ModuleServiceImplementation>();
            app.Run();
        }
    }
}
