using Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Data
{
    public class PlanServiceDbContext : DbContext
    {
        public PlanServiceDbContext(DbContextOptions<PlanServiceDbContext> options)
            : base(options) { }
          
        public DbSet<Plan> Plans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
