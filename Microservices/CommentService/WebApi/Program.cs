using Core.Interface;
using Core.MappingProfiles;
using Core.Services;
using Core.ServicesAbstraction;
using Core.Settings;
using FluentValidation;
using Infrastructure.Context;
using Infrastructure.gRPC.Plan;
using Infrastructure.gRPC.Team;
using Infrastructure.gRPC.UserQuests;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApi.Hubs;
using WebApi.Hubs.HubFilters;
using WebApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/comments"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

    var jwt = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
    };
});

builder.Services.AddSignalR(option =>
{
    option.AddFilter<CommentHubFilter>();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("CommentDbConnectionString"));
});
builder.Services.AddAutoMapper(cfg => { }, typeof(CommentProfile).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(AddCommentDtoValidator).Assembly, includeInternalTypes: true);
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserQuestGrpcService, UserQuestGrpcService>();
builder.Services.AddScoped<IGrpcPlanService, GrpcPlanService>();
builder.Services.AddScoped<IGrpcTeamService, GrpcTeamService>();

builder.Services.AddGrpcClient<GrpcUserQuestsService.GrpcUserQuestsServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:UserQuestsServiceUrl"] ?? throw new InvalidOperationException("gRPC service URL is missing."));
}).ConfigurePrimaryHttpMessageHandler(() =>
{

    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

builder.Services.AddGrpcClient<PlanServiceGrpc.PlanServiceGrpcClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:PlanServiceUrl"] ?? throw new InvalidOperationException("gRPC service URL is missing."));
}).ConfigurePrimaryHttpMessageHandler(() =>
{

    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:TeamServiceUrl"] ?? throw new InvalidOperationException("gRPC service URL is missing."));
}).ConfigurePrimaryHttpMessageHandler(() =>
{

    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<CommentHub>("hubs/comments");

app.UseStaticFiles();

app.MapControllers();

app.Run();
