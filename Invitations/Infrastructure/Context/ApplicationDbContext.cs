using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Invitation> Invitations { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var converter = new EnumToStringConverter<Status>();
        modelBuilder.Entity<Invitation>()
            .Property(i => i.Status)
            .HasConversion(converter);
    }
}
