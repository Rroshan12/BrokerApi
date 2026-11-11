using HouseBroker.Domain.Entities;
using HouseBroker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HouseBroker.Infrastructure.Persistence;

/// <summary>
/// INFRASTRUCTURE LAYER: EF Core context for MSSQL, including the ASP.NET Identity tables.
/// Entity mapping lives in Persistence/Configurations; only repositories and Identity use this class.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<PropertyListing> PropertyListings => Set<PropertyListing>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
