using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Data
{
    public class CompanyDbContext : DbContext
    {
        public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
           : base(options) { }

        public DbSet<Core.Models.Company> Companies => Set<Core.Models.Company>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);            
        }
    }
}
