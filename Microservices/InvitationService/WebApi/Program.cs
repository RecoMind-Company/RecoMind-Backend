using Core.Extensions;
using Hangfire;
using Infrastructure;
using Infrastructure.Extentions;
using WebApi.Extensions;
using WebApi.gRPC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();
builder.AddPresentationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("OpenCors");
app.MapControllers();
app.MapGrpcService<GrpcInvitationService>();
app.MapGrpcReflectionService();
app.UseHangfireDashboard("/dashboard");
SetupHangfireJobs.AddHangfireJobs();
app.Run();
