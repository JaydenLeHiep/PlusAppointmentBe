using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.Property(u => u.UserId).HasColumnName("user_id");
        builder.Property(u => u.Username).HasColumnName("username");
        builder.Property(u => u.Password).HasColumnName("password");
        builder.Property(u => u.Email).HasColumnName("email");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        builder.Property(u => u.Role).HasColumnName("role");
        builder.Property(u => u.Phone).HasColumnName("phone");
    }
}