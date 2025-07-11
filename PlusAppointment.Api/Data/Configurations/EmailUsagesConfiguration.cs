using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class EmailUsagesConfiguration : IEntityTypeConfiguration<EmailUsage>
{
    public void Configure(EntityTypeBuilder<EmailUsage> builder)
    {
        builder.ToTable("email_usage");
        
        builder.ToTable("email_usage");
        builder.Property(eu => eu.EmailUsageId).HasColumnName("email_usage_id");
        builder.Property(eu => eu.BusinessId).HasColumnName("business_id");
        builder.Property(eu => eu.Year).HasColumnName("year");
        builder.Property(eu => eu.Month).HasColumnName("month");
        builder.Property(eu => eu.EmailCount).HasColumnName("email_count");
        
        builder
            .HasOne(eu => eu.Business)
            .WithMany(b => b.EmailUsages)
            .HasForeignKey(eu => eu.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasIndex(eu => new { eu.BusinessId, eu.Year, eu.Month })
            .IsUnique();
    }
}