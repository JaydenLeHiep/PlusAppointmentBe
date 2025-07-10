using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Models.Enums;

namespace PlusAppointment.Data.Configurations;

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("check_ins"); 
        
        builder.Property(ci => ci.CheckInId).HasColumnName("check_in_id");
        builder.Property(ci => ci.CustomerId).HasColumnName("customer_id");
        builder.Property(ci => ci.BusinessId).HasColumnName("business_id");
        builder.Property(ci => ci.CheckInTime).HasColumnName("check_in_time");
        builder.Property(ci => ci.CheckInType)
            .HasColumnName("check_in_type")
            .HasConversion(
                v => v.ToString(),  // Convert Enum to string when saving
                v => (CheckInType)Enum.Parse(typeof(CheckInType), v)  // Convert string to Enum when reading
            );
        
        builder
            .HasOne(ci => ci.Customer)
            .WithMany(c => c.CheckIns)
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ci => ci.Business)
            .WithMany(b => b.CheckIns)
            .HasForeignKey(ci => ci.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(ci => ci.CustomerId)
            .HasDatabaseName("IX_CheckIn_CustomerId");

        builder
            .HasIndex(ci => ci.BusinessId)
            .HasDatabaseName("IX_CheckIn_BusinessId");

        builder
            .HasIndex(ci => ci.CheckInTime)
            .HasDatabaseName("IX_CheckIn_CheckInTime");
    }
}