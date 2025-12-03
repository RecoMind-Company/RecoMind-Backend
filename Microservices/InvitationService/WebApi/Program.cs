using Core.Extensions;
using Hangfire;
using Infrastructure;
using Infrastructure.Extentions;
using WebApi.Extensions;
using WebApi.gRPC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddPresentationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();


var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcInvitationService>();
app.MapGrpcReflectionService();
app.UseHangfireDashboard("/dashboard");
SetupHangfireJobs.AddHangfireJobs();
app.Run();
