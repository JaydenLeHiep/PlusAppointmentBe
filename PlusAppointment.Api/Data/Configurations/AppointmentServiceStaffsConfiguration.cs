using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class AppointmentServiceStaffsConfiguration : IEntityTypeConfiguration<AppointmentServiceStaffMapping>
{
    public void Configure(EntityTypeBuilder<AppointmentServiceStaffMapping> builder)
    {
        builder.ToTable("appointment_services_staffs");
        
        builder.Property(assm => assm.AppointmentId)
            .HasColumnName("appointment_id");
        builder.Property(assm => assm.ServiceId)
            .HasColumnName("service_id");
        builder.Property(assm => assm.StaffId)
            .HasColumnName("staff_id");
        
        builder
            .HasOne(assm => assm.Appointment)
            .WithMany(a => a.AppointmentServices)
            .HasForeignKey(assm => assm.AppointmentId);

        builder
            .HasOne(assm => assm.Service)
            .WithMany(s => s.AppointmentServicesStaffs)
            .HasForeignKey(assm => assm.ServiceId);

        builder
            .HasOne(assm => assm.Staff)
            .WithMany(s => s.AppointmentServicesStaffs)
            .HasForeignKey(assm => assm.StaffId);
        
        builder
            .HasKey(assm => new { assm.AppointmentId, assm.ServiceId, assm.StaffId });
    }
}