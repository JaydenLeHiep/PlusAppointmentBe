using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class ServiceCategoriesConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories");
        
        builder.ToTable("service_categories");
        builder.HasKey(sc => sc.CategoryId); // Explicitly define the primary key
        builder.Property(sc => sc.CategoryId).HasColumnName("category_id");
        builder.Property(sc => sc.Name).HasColumnName("name");
        builder.Property(sc => sc.Color).HasColumnName("color");
        
        builder
            .HasIndex(sc => sc.Color)
            .HasDatabaseName("IX_ServiceCategory_Color");
        
        
    }
}