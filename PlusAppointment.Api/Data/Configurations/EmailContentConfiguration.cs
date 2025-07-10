using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes.Emails;

namespace PlusAppointment.Data.Configurations;

public class EmailContentConfiguration : IEntityTypeConfiguration<EmailContent>
{
    public void Configure(EntityTypeBuilder<EmailContent> builder)
    {
        builder.ToTable("email_contents");
        
        builder.Property(e => e.EmailContentId)
            .HasColumnName("email_content_id")
            .IsRequired();

        builder.Property(e => e.Subject)
            .HasColumnName("subject")
            .IsRequired();

        builder.Property(e => e.Body)
            .HasColumnName("body")
            .IsRequired();
        
        builder
            .HasIndex(e => e.EmailContentId)
            .HasDatabaseName("IX_email_content_id");

        builder
            .HasIndex(e => e.Subject)
            .HasDatabaseName("IX_email_subject");
    }
}