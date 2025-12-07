using Core.Extentions;
using Infrastructure.Extentions;
using WebApi.CustomMiddlewares;
using WebApi.Extensions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddPresentationServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
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

app.UseCors("OpenCors");
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<CustomExceptionHandlerMiddleware>();
//app.UseHangfireDashboard("/dashboard");
app.Run();
