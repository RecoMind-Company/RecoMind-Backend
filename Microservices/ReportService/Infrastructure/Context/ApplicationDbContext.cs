using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Report> Reports { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var converter = new EnumToStringConverter<Periodic>();
        modelBuilder
            .Entity<Report>()
            .Property(r => r.Periodic)
            .HasConversion(converter);
    }
}
