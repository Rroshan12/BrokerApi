using HouseBroker.Domain.Entities;
using HouseBroker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBroker.Infrastructure.Persistence.Configurations;

public class PropertyListingConfiguration : IEntityTypeConfiguration<PropertyListing>
{
    public void Configure(EntityTypeBuilder<PropertyListing> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.City).HasMaxLength(100).IsRequired();
        b.Property(x => x.Area).HasMaxLength(100);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.AreaSqFt).HasPrecision(18, 2);
        b.Property(x => x.CommissionAmount).HasPrecision(18, 2);
        b.Property(x => x.CommissionRatePercent).HasPrecision(5, 3);
        b.PrimitiveCollection(x => x.Features);
        b.Property(x => x.BrokerId).IsRequired();

        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.BrokerId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Images).WithOne().HasForeignKey(i => i.PropertyListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for the search filters
        b.HasIndex(x => x.City);
        b.HasIndex(x => x.Price);
        b.HasIndex(x => x.PropertyType);
        b.ToTable(t => t.HasCheckConstraint("CK_Listing_Price", "[Price] > 0"));
    }
}

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> b) =>
        b.Property(x => x.Url).HasMaxLength(500).IsRequired();
}

public class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    public void Configure(EntityTypeBuilder<CommissionRule> b)
    {
        b.Property(x => x.MinPrice).HasPrecision(18, 2);
        b.Property(x => x.MaxPrice).HasPrecision(18, 2);
        b.Property(x => x.RatePercent).HasPrecision(5, 3);

        // Seed the bands from the assignment (INR): <50L 2%, 50L–1Cr 1.75%, >1Cr 1.5%
        b.HasData(
            new CommissionRule { Id = Guid.Parse("8a1f0000-0000-0000-0000-000000000001"), MinPrice = 0m, MaxPrice = 5_000_000m, RatePercent = 2.00m, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CommissionRule { Id = Guid.Parse("8a1f0000-0000-0000-0000-000000000002"), MinPrice = 5_000_000m, MaxPrice = 10_000_001m, RatePercent = 1.75m, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CommissionRule { Id = Guid.Parse("8a1f0000-0000-0000-0000-000000000003"), MinPrice = 10_000_001m, MaxPrice = null, RatePercent = 1.50m, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
    }
}
