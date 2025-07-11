using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data.Configurations;

public class OpeningHoursConfiguration : IEntityTypeConfiguration<OpeningHours>
{
    public void Configure(EntityTypeBuilder<OpeningHours> builder)
    {
        builder.ToTable("opening_hours");
        
        builder
                .Property(oh => oh.MondayOpeningTime).HasColumnName("monday_opening_time");

       builder
            .Property(oh => oh.MondayClosingTime).HasColumnName("monday_closing_time");

       builder
            .Property(oh => oh.TuesdayOpeningTime).HasColumnName("tuesday_opening_time");

       builder
            .Property(oh => oh.TuesdayClosingTime).HasColumnName("tuesday_closing_time");

       builder
            .Property(oh => oh.WednesdayOpeningTime).HasColumnName("wednesday_opening_time");

       builder
            .Property(oh => oh.WednesdayClosingTime).HasColumnName("wednesday_closing_time");

       builder
            .Property(oh => oh.ThursdayOpeningTime).HasColumnName("thursday_opening_time");

       builder
            .Property(oh => oh.ThursdayClosingTime).HasColumnName("thursday_closing_time");

       builder
            .Property(oh => oh.FridayOpeningTime).HasColumnName("friday_opening_time");

       builder
            .Property(oh => oh.FridayClosingTime).HasColumnName("friday_closing_time");

       builder
            .Property(oh => oh.SaturdayOpeningTime).HasColumnName("saturday_opening_time");

       builder
            .Property(oh => oh.SaturdayClosingTime).HasColumnName("saturday_closing_time");

       builder
            .Property(oh => oh.SundayOpeningTime).HasColumnName("sunday_opening_time");

       builder
            .Property(oh => oh.SundayClosingTime).HasColumnName("sunday_closing_time");

       builder
            .Property(oh => oh.MinimumAdvanceBookingMinutes).HasColumnName("minimum_advance_booking_minutes");
       builder
            .Property(oh => oh.Id).HasColumnName("id");

       builder
            .Property(oh => oh.BusinessId).HasColumnName("business_id");
       
       
       builder
            .HasOne(oh => oh.Business)
            .WithMany(b => b.OpeningHours)
            .HasForeignKey(oh => oh.BusinessId);
       
       builder
            .HasIndex(oh => oh.BusinessId).HasDatabaseName("IX_OpeningHours_BusinessId");
    }
}