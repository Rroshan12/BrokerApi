using HouseBroker.Application.Commission;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Enums;
using HouseBroker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBroker.Infrastructure.Persistence.Seed;

/// <summary>
/// Applies pending migrations and, in Development only, adds demo accounts and listings
/// so the API can be tried from Swagger straight away.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>Demo password for the seeded development accounts. Never used outside Development.</summary>
    public const string DemoPassword = "Demo@12345";
    public const string DemoBrokerEmail = "broker@demo.local";
    public const string DemoSeekerEmail = "seeker@demo.local";

    public static async Task InitialiseDatabaseAsync(this IServiceProvider services, bool seedDemoData, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);

        if (seedDemoData && !await db.Users.AnyAsync(ct))
            await SeedDemoDataAsync(scope.ServiceProvider, db, ct);
    }

    private static async Task SeedDemoDataAsync(IServiceProvider sp, AppDbContext db, CancellationToken ct)
    {
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var commission = sp.GetRequiredService<ICommissionCalculator>();

        var broker = await CreateUserAsync(users, DemoBrokerEmail, "Asha Broker", "+977 9800000001", "Demo Realty", Roles.Broker);
        await CreateUserAsync(users, DemoSeekerEmail, "Sam Seeker", null, null, Roles.Seeker);

        PropertyListing[] listings =
        [
            new() { Title = "2BHK apartment near the ring road", Description = "Bright corner flat with lift and backup power.", PropertyType = PropertyType.Apartment, Price = 3_500_000m, City = "Kathmandu", Area = "Baneshwor", Address = "Ring Road, Baneshwor", Bedrooms = 2, Bathrooms = 2, AreaSqFt = 950m, Features = ["Parking", "Lift"] },
            new() { Title = "Family house with garden", Description = "Three-storey house on a quiet street.", PropertyType = PropertyType.House, Price = 8_500_000m, City = "Lalitpur", Area = "Jhamsikhel", Address = "Jhamsikhel Road", Bedrooms = 4, Bathrooms = 3, AreaSqFt = 2200m, Features = ["Parking", "Garden"] },
            new() { Title = "Commercial space on the main road", Description = "Ground-floor shop front with storage.", PropertyType = PropertyType.Commercial, Price = 25_000_000m, City = "Kathmandu", Area = "New Road", Address = "New Road", Bedrooms = 0, Bathrooms = 1, AreaSqFt = 1800m, Features = ["Main road"] },
            new() { Title = "Residential plot", Description = "Flat land with road access.", PropertyType = PropertyType.Land, Price = 6_000_000m, City = "Bhaktapur", Area = "Suryabinayak", Address = "Suryabinayak", Bedrooms = 0, Bathrooms = 0, AreaSqFt = 3400m, Features = [] }
        ];

        foreach (var listing in listings)
        {
            listing.BrokerId = broker.Id;
            listing.ReplaceImages([$"https://picsum.photos/seed/{listing.Id:N}/800/600"]);
            var result = await commission.CalculateAsync(listing.Price, ct);
            listing.SetCommission(result.RatePercent, result.Amount);
        }

        db.PropertyListings.AddRange(listings);
        await db.SaveChangesAsync(ct);
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> users, string email, string name, string? phone, string? agency, string role)
    {
        var user = new ApplicationUser { UserName = email, Email = email, FullName = name, PhoneNumber = phone, AgencyName = agency };
        var created = await users.CreateAsync(user, DemoPassword);
        if (!created.Succeeded)
            throw new InvalidOperationException($"Could not seed demo user {email}: {string.Join("; ", created.Errors.Select(e => e.Description))}");
        await users.AddToRoleAsync(user, role);
        return user;
    }
}
