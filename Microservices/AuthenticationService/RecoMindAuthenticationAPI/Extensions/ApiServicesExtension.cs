using Authentication.Infrastructure.gRPC.Protos;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

namespace RecoMindAuthenticationAPI.Extensions
{
    public static class ApiServicesExtension
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
            builder.Services.AddGrpc();
            builder.Services.AddGrpcReflection();
            builder.Services.AddGrpcClient<InvitationService.InvitationServiceClient>(c =>
            {
                c.Address = new Uri(configuration["Urls:InvitationServiceUrl"]);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                
                
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                
                return new HttpClientHandler();
            });
        }
        public static StaticFileOptions AddMyStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            string physicalPathToSave = Path.Combine(env.ContentRootPath, "StaticFiles", "Images");
            if (!Directory.Exists(physicalPathToSave))
                Directory.CreateDirectory(physicalPathToSave);
            StaticFileOptions staticFileOptions = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "StaticFiles", "Images")),
                RequestPath = "/UserProfileImage"
            };
            return staticFileOptions;
        }
    }
}
