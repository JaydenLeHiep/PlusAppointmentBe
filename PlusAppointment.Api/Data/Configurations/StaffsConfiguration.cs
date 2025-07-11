using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class StaffsConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("staffs");
        
        builder.Property(s => s.StaffId).HasColumnName("staff_id");
        builder.Property(s => s.BusinessId).HasColumnName("business_id");
        builder.Property(s => s.Name).HasColumnName("name");
        builder.Property(s => s.Email).HasColumnName("email");
        builder.Property(s => s.Phone).HasColumnName("phone");
        builder.Property(s => s.Password).HasColumnName("password");
        
        builder
            .HasOne(s => s.Business)
            .WithMany(b => b.Staffs)
            .HasForeignKey(s => s.BusinessId);
        
        builder
            .HasIndex(s => s.BusinessId);
        
    }
}