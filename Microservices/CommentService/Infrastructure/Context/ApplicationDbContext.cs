using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<PlanComment> PlanComments { get; set; } = default!;
    public DbSet<QuestComment> QuestComments { get; set; }
}
