
using Core.Configuration;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Core.Service.Protos;
using Infrastructure.Data;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using webApi.Grpc.GrpcImplementations;
using webApi.Mapping;


namespace Campany.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();


            builder.Services.AddDbContext<CompanyDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("ProductionConnection_Company"),
                        sqlOptions =>
                        sqlOptions.MigrationsAssembly(typeof(CompanyDbContext).Assembly.FullName)
                        ));

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(unitOfWork<>));
            builder.Services.AddScoped(typeof(ICompanyService), typeof(Core.Service.CompanyService));
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAutoMapper(typeof(CopmanyMapping), typeof(MappingForRpc));

            builder.Services.AddLogging();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add gRPC services to the container. 
            builder.Services.AddGrpc();

            builder.Services.AddGrpcClient<subscriptionService.subscriptionServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration.GetValue<string>("GrpcSettings:SubscriptionServiceUrl"));              // Subscription service address
            });

            //Configure Kestrel to listen on a specific port for gRPC


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

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddGrpcReflection();
            }

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
            app.MapControllers();

            app.MapGrpcService<CompanyServiceImpl>();
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            app.Run();
        }
    }
}
