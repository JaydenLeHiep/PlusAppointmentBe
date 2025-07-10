using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class CustomersConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        
        builder.Property(c => c.CustomerId).HasColumnName("customer_id");
        builder.Property(c => c.Name).HasColumnName("name");
        builder.Property(c => c.Email).HasColumnName("email");
        builder.Property(c => c.Phone).HasColumnName("phone");
        builder.Property(c => c.Birthday).HasColumnName("birthday");
        builder.Property(c => c.WantsPromotion).HasColumnName("wants_promotion");
        builder.Property(c => c.Note).HasColumnName("note");
        builder.Property(c => c.BusinessId).HasColumnName("business_id");
        
        builder
            .HasOne(c => c.Business)
            .WithMany(b => b.Customers)
            .HasForeignKey(c => c.BusinessId);
        
        builder
            .HasIndex(c => c.BusinessId);
    }
}