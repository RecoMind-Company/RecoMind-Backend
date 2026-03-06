using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class PlanDbContext : DbContext
    {
        public PlanDbContext(DbContextOptions<PlanDbContext> options) : base(options)
        {
        }

        public DbSet<Plan> Plans { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
    
                // Configure the Plan entity
                modelBuilder.Entity<Plan>(entity =>
                {
                    entity.HasKey(p => p.Id); // Set Id as the primary key
    
                    // Configure properties
                    entity.Property(p => p.Goal).IsRequired();
                    entity.Property(p => p.Description).IsRequired();
                    entity.Property(p => p.Status).IsRequired();
                    entity.Property(p => p.PlanType).IsRequired();
                    entity.Property(p => p.IsApproved).IsRequired();
                    entity.Property(p => p.Duration).IsRequired();
                    entity.Property(p => p.StartDate).IsRequired();
                    entity.Property(p => p.EndDate).IsRequired();
    
                    // Configure relationships if needed (e.g., with User, Company, Team)
                });
        }
    }
}
