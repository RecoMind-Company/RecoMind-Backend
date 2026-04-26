using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Notification.Core.Infrastructure.Data;
using Notification.Core.Interfaces;
using Notification.Core.Services;
using Notification.Infrastructure;
using Notification.Infrastructure.Repositories;
using Notification.WebApi.Hubs;
using System.Text;

namespace Notification.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Database Configuration (نفس طريقة سيرفيس Team)
            builder.Services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionConnection_Notification"),

                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)
                )
            );

            // 2. Add Services & Repositories
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationHubContext, NotificationHubContext>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //builder.Services.AddHostedService<RabbitMQConsumer>(); // مش هستخدمه دلوقتي عشان MassTransit هيقوم بكل حاجة
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<NotificationConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    // بنقرأ الداتا من الـ Configuration اللي أنت بعته
                    var rabbitSettings = builder.Configuration.GetSection("RabbitMQ");

                    cfg.Host(rabbitSettings["Host"] ?? "localhost", h =>
                    {
                        h.Username(rabbitSettings["Username"] ?? "guest");
                        h.Password(rabbitSettings["Password"] ?? "guest");
                    });

                    cfg.ReceiveEndpoint("notification-queue", e =>
                    {
                        e.ConfigureConsumer<NotificationConsumer>(context);
                    });
                });
            });

            // Add SignalR
            builder.Services.AddSignalR();

            // 3. Configure CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("OpenCors", policy =>
                {
                    //policy
                    //    .AllowAnyOrigin()     // يسمح بأي دومين
                    //    .AllowAnyHeader()     // يسمح بأي هيدر
                    //    .AllowAnyMethod();    // يسمح بأي نوع HTTP Method

                    policy.SetIsOriginAllowed(origin => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // لازم دي عشان الـ SignalR يشتغل
                });
            });

            // 4. JWT Authentication
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
                    ClockSkew = TimeSpan.Zero
                };

                // SignalR Token Handling
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // 5. Authorization Policies (توحيد مع باقي الميكروسيرفس)
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AllEmployees", p => p.RequireRole("admin", "manager", "teamleader", "employee"));
                options.AddPolicy("TeamLeadership", p => p.RequireRole("admin", "manager", "teamleader"));
                options.AddPolicy("Management", p => p.RequireRole("admin", "manager"));
            });

            // 6. Controllers & Swagger
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

            builder.WebHost.ConfigureKestrel(options =>
            {
                var httpPort = int.Parse(
                    Environment.GetEnvironmentVariable("HTTP_PORT") ??
                    builder.Configuration["Kestrel:Endpoints:Http:Port"] ??
                    "8001"
                );

                options.ListenAnyIP(httpPort);
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("OpenCors");
            app.UseAuthentication();
            app.UseAuthorization();

            // 7. Map Endpoints & Hubs
            //app.MapHub<NotificationHub>("/hubs/notifications");
            app.MapHub<NotificationHub>("/hubs/notifications").AllowAnonymous();

            app.MapControllers();

            app.Run();
        }
    }
}