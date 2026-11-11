using HouseBroker.Application.Auth;
using HouseBroker.Application.Commission;
using HouseBroker.Application.Listings;
using HouseBroker.Infrastructure.Identity;
using HouseBroker.Infrastructure.Persistence;
using HouseBroker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBroker.Infrastructure;

/// <summary>
/// INFRASTRUCTURE LAYER: EF Core/MSSQL, ASP.NET Identity, JWT issuing and repositories.
/// Implements the interfaces declared in Application and holds no business rules.
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
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddOptions<JwtOptions>().Bind(config.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IdentityService>();
        services.AddScoped<IIdentityService>(sp => sp.GetRequiredService<IdentityService>());
        services.AddScoped<IBrokerDirectory>(sp => sp.GetRequiredService<IdentityService>());
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<ICommissionRuleRepository, CommissionRuleRepository>();
        return services;
    }
}
