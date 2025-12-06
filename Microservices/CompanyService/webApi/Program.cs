
using Core.Configuration;
using Core.Interfaces;
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

            builder.Services.AddDbContext<CompanyDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("ProdcutionConnection_Company"),
                        sqlOptions =>
                        sqlOptions.MigrationsAssembly(typeof(CompanyDbContext).Assembly.FullName)
                        ));

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(unitOfWork<>));
            builder.Services.AddScoped(typeof(ICompanyService), typeof(Core.Service.CompanyService));

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
                o.Address = new Uri("http://subscriptionservice:5004");              // Subscription service address
            });

            builder.Services.AddGrpcClient<RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient>(o =>
            {
                o.Address = new Uri("http://authenticationservice:5011");            // AuthenticationService service address
            });

            // Configure Kestrel to listen on a specific port for gRPC

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    // اقرأ من environment أولاً (أولوية أعلى)
            //    var httpPort = int.Parse(
            //        Environment.GetEnvironmentVariable("HTTP_PORT") ??
            //        Environment.GetEnvironmentVariable("Kestrel_EndpointsHttp_Port") ??
            //        builder.Configuration["Kestrel:Endpoints:Http:Port"] ??
            //        "8001"
            //    );

            //    var grpcPort = int.Parse(
            //        Environment.GetEnvironmentVariable("GRPC_PORT") ??
            //        Environment.GetEnvironmentVariable("Kestrel_EndpointsGrpc_Port") ??
            //        builder.Configuration["Kestrel:Endpoints:Grpc:Port"] ??
            //        "5001"
            //    );

            //    options.ListenAnyIP(httpPort, o => o.Protocols = HttpProtocols.Http1);
            //    options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
            //});

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
                    ClockSkew = TimeSpan.Zero, // ONLY FOR TESTING
                };
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddGrpcReflection();
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();


            // app.UseHttpsRedirection();

            app.UseAuthorization();

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
