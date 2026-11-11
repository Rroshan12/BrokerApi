using HouseBroker.Infrastructure.Identity;
using HouseBroker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBroker.Infrastructure;

/// <summary>
/// INFRASTRUCTURE LAYER: EF Core/MSSQL, ASP.NET Identity, caching, file storage.
/// Implements the interfaces declared in Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Set it with: " +
                "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<your connection string>\"");

        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddMemoryCache();
        // services.AddScoped<IListingRepository, ListingRepository>();
        // services.AddScoped<ICacheService, MemoryCacheService>();
        return services;
    }
}
