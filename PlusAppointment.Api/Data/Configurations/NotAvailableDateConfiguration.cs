using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class NotAvailableDateConfiguration : IEntityTypeConfiguration<NotAvailableDate>
{
    public void Configure(EntityTypeBuilder<NotAvailableDate> builder)
    {
        builder.ToTable("not_available_dates");
        
        builder.Property(nad => nad.NotAvailableDateId)
            .HasColumnName("not_available_date_id");
        builder.Property(nad => nad.BusinessId).HasColumnName("business_id");
        builder.Property(nad => nad.StaffId).HasColumnName("staff_id");
        builder.Property(nad => nad.StartDate).HasColumnName("start_date");
        builder.Property(nad => nad.EndDate).HasColumnName("end_date");
        builder.Property(nad => nad.Reason).HasColumnName("reason");
        
        
        builder
            .HasOne(nad => nad.Business)
            .WithMany(b => b.NotAvailableDates)
            .HasForeignKey(nad => nad.BusinessId);
        
        builder
            .HasIndex(nad => nad.BusinessId);
    }
}