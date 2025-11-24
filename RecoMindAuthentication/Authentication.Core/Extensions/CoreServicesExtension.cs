using Authentication.Core.Models;
using Authentication.Core.Services;
using Authentication.Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Authentication.Core.Extensions;

public static class CoreServicesExtension
{
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IVerificationEmailService, VerificationEmailService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<PasswordHasher<AppUser>>();
        services.Configure<JwtSettings>(configuration.GetSection("JwtOptions"));
        services.Configure<EmailConfgSettings>(configuration.GetSection("email-config"));
        services.Configure<PhotoSettings>(configuration.GetSection("Urls"));
        services.AddAuthentication(config =>
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
    }
}
