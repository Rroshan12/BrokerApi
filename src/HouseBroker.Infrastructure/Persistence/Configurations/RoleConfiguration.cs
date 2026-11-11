using HouseBroker.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBroker.Infrastructure.Persistence.Configurations;

/// <summary>Seeds the two Identity roles that separate house seekers from brokers.</summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> b) =>
        b.HasData(
            new IdentityRole
            {
                Id = "5f0c1b3e-0000-0000-0000-000000000001",
                Name = Roles.Broker,
                NormalizedName = Roles.Broker.ToUpperInvariant(),
                ConcurrencyStamp = "5f0c1b3e-0000-0000-0000-0000000000a1"
            },
            new IdentityRole
            {
                Id = "5f0c1b3e-0000-0000-0000-000000000002",
                Name = Roles.Seeker,
                NormalizedName = Roles.Seeker.ToUpperInvariant(),
                ConcurrencyStamp = "5f0c1b3e-0000-0000-0000-0000000000a2"
            });
}
