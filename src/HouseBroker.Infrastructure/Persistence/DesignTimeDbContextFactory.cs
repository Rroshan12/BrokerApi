using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HouseBroker.Infrastructure.Persistence;

/// <summary>
/// Used only by the <c>dotnet ef</c> tools when the Api host cannot start (e.g. secrets not set yet).
/// Adding a migration never connects to a database. Commands that do connect use the
/// HOUSEBROKER_DESIGN_CONNECTION environment variable, or LocalDB if it is not set.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("HOUSEBROKER_DESIGN_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=HouseBroker;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
        return new AppDbContext(options);
    }
}
