using Core.Extentions;
using Infrastructure.Extentions;
using WebApi.CustomMiddlewares;
using WebApi.Extensions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddPresentationServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<CustomExceptionHandlerMiddleware>();
//app.UseHangfireDashboard("/dashboard");
app.Run();
