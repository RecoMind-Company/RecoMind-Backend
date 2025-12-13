using Microsoft.EntityFrameworkCore;
using Team.Core.Models;

namespace Team.Infrastructure.Data
{
    public class TeamDbContext : DbContext
    {
        public TeamDbContext(DbContextOptions<TeamDbContext> options)
            : base(options) { }

        public DbSet<TeamModel> Teams { get; set; }
        public DbSet<TeamEmployee> TeamEmployees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TeamEmployee>()
                .HasOne(te => te.Team)
                .WithMany(t => t.TeamEmployees)
                .HasForeignKey(te => te.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
