using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class AppointmentsConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        
        builder.Property(a => a.AppointmentId).HasColumnName("appointment_id");
        builder.Property(a => a.CustomerId).HasColumnName("customer_id");
        builder.Property(a => a.BusinessId).HasColumnName("business_id");
        builder.Property(a => a.AppointmentTime).HasColumnName("appointment_time");
        builder.Property(a => a.Duration).HasColumnName("duration");
        builder.Property(a => a.Status).HasColumnName("status");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        builder.Property(a => a.Comment).HasColumnName("comment");
        
        builder
            .HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId);

        builder
            .HasOne(a => a.Business)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BusinessId);
        
       builder
            .HasIndex(a => a.CustomerId);

        builder
            .HasIndex(a => a.BusinessId);
        builder
            .HasIndex(a => a.CustomerId);

        builder
            .HasIndex(a => a.BusinessId);
    }
}