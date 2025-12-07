using Authentication.Core.Extensions;
using Authentication.Infrastructure;
using Authentication.Infrastructure.Extensions;
using RecoMindAuthenticationAPI.Extensions;
using RecoMindAuthenticationAPI.Grpc.Authentication.Service;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddPresentationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration);

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

using var Scope = app.Services.CreateScope();
var ObjectOfDataSeeding = Scope.ServiceProvider.GetRequiredService<DataSeeding>();
await ObjectOfDataSeeding.DataSeedAsync();
// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles(ApiServicesExtension.AddMyStaticFiles(app, builder.Environment));

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("OpenCors");
app.MapControllers();
app.MapGrpcService<GrpcAuthenticationService>();
app.MapGrpcReflectionService();
app.Run();
