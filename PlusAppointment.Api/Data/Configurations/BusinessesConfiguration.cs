using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes.Business;

namespace PlusAppointment.Data.Configurations;

public class BusinessesConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("businesses");
        
       builder.Property(b => b.BusinessId).HasColumnName("business_id");
       builder.Property(b => b.Name).HasColumnName("name");
       builder.Property(b => b.Address).HasColumnName("address");
       builder.Property(b => b.Phone).HasColumnName("phone");
       builder.Property(b => b.Email).HasColumnName("email");
       builder.Property(b => b.UserID).HasColumnName("user_id");
       builder.Property(b => b.RequiresAppointmentConfirmation)
            .HasColumnName("requires_appointment_confirmation")
            .HasDefaultValue(false);
        builder.Property(b => b.BirthdayDiscountPercentage)
            .HasColumnName("birthday_discount_percentage")
            .HasDefaultValue(0);
        
        builder
            .HasOne(b => b.User)
            .WithMany(u => u.Businesses)
            .HasForeignKey(b => b.UserID);
        
        builder
            .HasIndex(b => b.UserID);

    }
}