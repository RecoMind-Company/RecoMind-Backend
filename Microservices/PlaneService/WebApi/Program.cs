using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.OpenApi.Models;
using webApi.Grpc;
using Core.Interfaces;
using Infrastructure.UnitOfWork;
using Core.Service.Interface;
using Core.Service;
using Core.Mapping;
using GrpcClients.Team;
using System;
using Infrastructure.GrpcClients.Team;

namespace webApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<PlanDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddGrpc();

            builder.Services.AddScoped<PlanServiceImpl>();
            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UOW<>));
            builder.Services.AddScoped(typeof(IPlanService),typeof(Core.Service.PlanService));
            builder.Services.AddScoped(typeof(IPlanType),typeof(PlanType));
            builder.Services.AddScoped(typeof(IStatus), typeof(Status));
            builder.Services.AddScoped<ITeamGrpcClient, TeamGrpcClientImpl>();
            builder.Services.AddAutoMapper(typeof(PlanMapper));

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(options =>
            {
                options.Address = new Uri(builder.Configuration.GetValue<string>("GrpcSettings:TeamServiceUrl")); //https://localhost:7192
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

            //builder.Services.AddSwaggerGen(cfg =>
            //{
            //    cfg.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
            //    {
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        In = ParameterLocation.Header,
            //    });
            //    cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //       {
            //       new OpenApiSecurityScheme
            //           {
            //               Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BearerAuth" }
            //           },
            //           []
            //       }
            //    });
            //});

            //builder.Services.AddAuthentication(config =>
            //{
            //    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    var jwt = builder.Configuration
            //    .GetSection("JwtOptions")
            //    .Get<JwtSettings>()
            //    ?? throw new Exception("JwtOptions are not configured");

            //    options.SaveToken = true;
            //    options.RequireHttpsMetadata = true;
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateIssuerSigningKey = true,

            //        ValidIssuer = jwt.Issuer,
            //        ValidAudience = jwt.Audience,

            //        IssuerSigningKey = new SymmetricSecurityKey(
            //         Encoding.UTF8.GetBytes(jwt.SecretKey)
            //         ),

            //        ClockSkew = TimeSpan.Zero, // ONLY FOR TESTING
            //    };
            //});

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
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("OpenCors");

            app.MapControllers();

            app.MapGrpcService<PlanServiceImpl>();

            app.Run();
        }
    }
}
