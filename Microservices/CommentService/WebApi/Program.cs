using Core.Interface;
using Core.MappingProfiles;
using Core.Services;
using Core.ServicesAbstraction;
using FluentValidation;
using Infrastructure.Context;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using WebApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});
builder.Services.AddAutoMapper(cfg => { }, typeof(CommentProfile).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(AddCommentDtoValidator).Assembly, includeInternalTypes: true);
builder.Services.AddScoped<ICommentService, CommentService>();
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

app.Run();
