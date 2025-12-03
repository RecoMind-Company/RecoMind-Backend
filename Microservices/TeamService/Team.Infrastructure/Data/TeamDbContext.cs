using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.Models;

namespace Team.Infrastructure.Data
{
    public class TeamDbContext : DbContext
    {
        public TeamDbContext(DbContextOptions<TeamDbContext> options)
            : base(options) { }

        public DbSet<Core.Models.TeamModel> Teams { get; set; }
        public DbSet<TeamEmployee> TeamEmployees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Unique index: CompanyId + Name (case-insensitive) and only for non-deleted rows (SQL Server syntax)
            modelBuilder.Entity<Core.Models.TeamModel>()
            .HasIndex(t => new { t.CompanyId, t.Name })
            .IsUnique()
            .HasDatabaseName("IX_Teams_CompanyId_Name");


            // Configure TeamEmployee
            modelBuilder.Entity<TeamEmployee>()
            .HasIndex(te => new { te.TeamId, te.EmployeeId })
            .IsUnique();


            modelBuilder.Entity<TeamEmployee>()
            .HasOne(te => te.Team)
            .WithMany(t => t.TeamEmployees)
            .HasForeignKey(te => te.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
