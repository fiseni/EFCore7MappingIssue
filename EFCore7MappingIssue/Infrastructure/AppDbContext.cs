using EFCore7MappingIssue.Domain;
using Microsoft.EntityFrameworkCore;

namespace EFCore7MappingIssue.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Campaign> Camapaigns => Set<Campaign>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureCustomConventions();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ConfigureCustomRules(this);
}
