using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class NotificationsConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification_table");
        
        builder.Property(n => n.NotificationId).HasColumnName("notification_id");
        builder.Property(n => n.BusinessId).HasColumnName("business_id");
        builder.Property(n => n.Message).HasColumnName("message");
        builder.Property(n => n.IsSeen).HasColumnName("is_seen").HasDefaultValue(false);
        builder.Property(n => n.NotificationType)
            .HasColumnName("notification_type")
            .HasConversion(
                v => v.ToString(), // Convert Enum to string when saving
                v => (NotificationType)Enum.Parse(typeof(NotificationType),
                    v) // Convert string to Enum when reading
            );
        builder.Property(n => n.CreatedAt).HasColumnName("created_at");
        
        builder
            .HasOne(n => n.Business)
            .WithMany(b => b.Notifications)
            .HasForeignKey(n => n.BusinessId);
        
        builder
            .HasIndex(n => n.BusinessId)
            .HasDatabaseName("IX_Notification_BusinessId");
    }
}