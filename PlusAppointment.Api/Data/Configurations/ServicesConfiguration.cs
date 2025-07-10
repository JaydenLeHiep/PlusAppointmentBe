using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class ServicesConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        
        builder.Property(s => s.ServiceId).HasColumnName("service_id");
        builder.Property(s => s.Name).HasColumnName("name");
        builder.Property(s => s.Description).HasColumnName("description");
        builder.Property(s => s.Duration).HasColumnName("duration");
        builder.Property(s => s.Price).HasColumnName("price");
        builder.Property(s => s.BusinessId).HasColumnName("business_id");
        builder.Property(s => s.CategoryId).HasColumnName("category_id");
        
        builder
            .HasOne(s => s.Category)
            .WithMany(sc => sc.Services)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder
            .HasOne(s => s.Business)
            .WithMany(b => b.Services)
            .HasForeignKey(s => s.BusinessId);

        builder
            .HasIndex(s => s.BusinessId);
    }
}