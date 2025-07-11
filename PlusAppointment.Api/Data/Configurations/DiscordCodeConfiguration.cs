using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Data.Configurations;

public class DiscordCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.ToTable("discount_codes");
        
        builder.Property(dc => dc.DiscountCodeId).HasColumnName("discount_code_id");
        builder.Property(dc => dc.Code).HasColumnName("code").IsRequired();
        builder.Property(dc => dc.DiscountPercentage).HasColumnName("discount_percentage");
        builder.Property(dc => dc.IsUsed).HasColumnName("is_used");
        builder.Property(dc => dc.GeneratedAt).HasColumnName("generated_at");
        
        
    }
}