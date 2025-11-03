
using Core.Configuration;
using Core.Interfaces;
using Core.Service;
using Core.Service.Interface;
using Infrastructure.Data;
using Infrastructure.Service;
using Infrastructure.Service.Interface;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Campany.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<CompanyDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    options => options.MigrationsAssembly(typeof(CompanyDbContext).Assembly.FullName));
            });

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            builder.Services.AddScoped(typeof(ICompanyService), typeof(CompanyService));
            builder.Services.AddScoped(typeof(IPlaneService), typeof(PlaneService));
            builder.Services.AddScoped(typeof(IBillingCycleServiice), typeof(BillingCycleService));
            builder.Services.AddAutoMapper(typeof(CopmanyMapping));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


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
        }
    }
}
