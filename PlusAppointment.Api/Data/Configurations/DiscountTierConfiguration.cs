using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Data.Configurations;

public class DiscountTierConfiguration : IEntityTypeConfiguration<DiscountTier>
{
    public void Configure(EntityTypeBuilder<DiscountTier> builder)
    {
        builder.ToTable("discount_tiers");
        
       builder.Property(dt => dt.DiscountTierId).HasColumnName("discount_tier_id");
       builder.Property(dt => dt.BusinessId).HasColumnName("business_id");
       builder.Property(dt => dt.CheckInThreshold).HasColumnName("check_in_threshold");
       builder.Property(dt => dt.DiscountPercentage).HasColumnName("discount_percentage");

       builder
            .HasOne(dt => dt.Business)
            .WithMany(b => b.DiscountTiers)  // Use `DiscountTiers` (plural) for the collection navigation property
            .HasForeignKey(dt => dt.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
       
       builder
           .HasIndex(dt => dt.BusinessId)
           .HasDatabaseName("IX_DiscountTier_BusinessId");
    }
}