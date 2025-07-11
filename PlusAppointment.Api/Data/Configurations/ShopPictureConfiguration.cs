using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class ShopPictureConfiguration : IEntityTypeConfiguration<ShopPicture>
{
    public void Configure(EntityTypeBuilder<ShopPicture> builder)
    {
        builder.ToTable("shop_pictures");
        
        builder.Property(sp => sp.ShopPictureId).HasColumnName("shop_picture_id");
        builder.Property(sp => sp.S3ImageUrl).HasColumnName("s3_image_url");
        builder.Property(sp => sp.CreatedAt).HasColumnName("created_at");
        builder.Property(sp => sp.BusinessId).HasColumnName("business_id");

        builder
            .HasOne(sp => sp.Business)
            .WithMany(b => b.ShopPictures)
            .HasForeignKey(sp => sp.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasIndex(sp => sp.BusinessId)
            .HasDatabaseName("IX_ShopPictures_BusinessId");

    }
}