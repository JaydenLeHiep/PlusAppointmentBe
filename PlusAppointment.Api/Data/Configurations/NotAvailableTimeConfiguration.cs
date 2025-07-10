using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class NotAvailableTimeConfiguration : IEntityTypeConfiguration<NotAvailableTime>
{
    public void Configure(EntityTypeBuilder<NotAvailableTime> builder)
    {
        builder.ToTable("not_available_times");
        
        builder.Property(nat => nat.NotAvailableTimeId)
            .HasColumnName("not_available_time_id");
        builder.Property(nat => nat.StaffId).HasColumnName("staff_id");
        builder.Property(nat => nat.BusinessId).HasColumnName("business_id");
        builder.Property(nat => nat.Date).HasColumnName("date");
        builder.Property(nat => nat.From).HasColumnName("from");
        builder.Property(nat => nat.To).HasColumnName("to");
        builder.Property(nat => nat.Reason).HasColumnName("reason");
        
        builder
            .HasOne(nat => nat.Business)
            .WithMany(b => b.NotAvailableTimes)
            .HasForeignKey(nat => nat.BusinessId);
        
        builder
            .HasIndex(nat => nat.StaffId)
            .HasDatabaseName("IX_NotAvailableTime_StaffId");

        builder
            .HasIndex(nat => nat.BusinessId)
            .HasDatabaseName("IX_NotAvailableTime_BusinessId");

        builder
            .HasIndex(nat => new { nat.Date, nat.From, nat.To })
            .HasDatabaseName("IX_NotAvailableTime_DateRange");
    }
}