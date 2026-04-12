using Core.Interfaces;
using Core.MappingProfiles;
using Core.Services;
using Core.ServicesAbstractions;
using Core.Settings;
using FluentValidation;
using Infrastructure.Context;
using Infrastructure.gRPC.Plan;
using Infrastructure.gRPC.Team;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.gRPC.UserQuests;
using WebApi.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(jo => jo.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
    var jwt = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
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
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});
builder.Services.AddAutoMapper(cfg => { }, typeof(QuestProfile).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(QuestDtoValidator).Assembly, includeInternalTypes: true);
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IUserQuestsService, UserQuestsService>();
builder.Services.AddScoped<IGrpcPlanService, GrpcPlanService>();
builder.Services.AddScoped<IGrpcTeamService, GrpcTeamService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddGrpc();

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

app.MapControllers();

app.MapGrpcService<UserQuestGrpcService>();

app.Run();

public partial class Program { }