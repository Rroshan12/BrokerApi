using HouseBroker.Domain.Entities;
using HouseBroker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBroker.Infrastructure.Persistence.Configurations;

/// <summary>Table, constraints and search indexes for listings.</summary>
public sealed class PropertyListingConfiguration : IEntityTypeConfiguration<PropertyListing>
{
    public void Configure(EntityTypeBuilder<PropertyListing> b)
    {
        // Ids are created in code (BaseEntity), so EF must treat new children as inserts.
        b.Property(x => x.Id).ValueGeneratedNever();
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
        b.HasIndex(x => new { x.BrokerId, x.CreatedAtUtc });

        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Listing_Price", "[Price] > 0");
            t.HasCheckConstraint("CK_Listing_Rooms", "[Bedrooms] >= 0 AND [Bathrooms] >= 0");
            t.HasCheckConstraint("CK_Listing_AreaSqFt", "[AreaSqFt] >= 0");
        });
    }
}
