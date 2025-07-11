using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class UserRefreshTokensConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("user_refresh_tokens");
        
        builder.Property(urt => urt.Id).HasColumnName("id");
        builder.Property(urt => urt.UserId).HasColumnName("user_id");
        builder.Property(urt => urt.Token).HasColumnName("token");
        builder.Property(urt => urt.ExpiryTime).HasColumnName("expiry_time");
        
        builder
            .HasIndex(urt => urt.Token)
            .IsUnique();
    }
}