using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Quest> Quests { get; set; }
    public DbSet<UserQuests> UserQuests { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserQuests>()
            .HasKey(uq => new { uq.UserId, uq.QuestId });
    }
}
