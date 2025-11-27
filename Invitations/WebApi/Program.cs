using Core.Extensions;
using Infrastructure.Extentions;
using WebApi.Extensions;
using WebApi.gRPC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddPresentationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcInvitationService>();
app.MapGrpcReflectionService();
app.Run();
