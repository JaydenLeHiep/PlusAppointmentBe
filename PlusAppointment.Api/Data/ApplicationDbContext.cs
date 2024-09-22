using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AppointmentServiceStaffMapping> AppointmentServiceStaffs { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<NotAvailableDate> NotAvailableDates { get; set; }
        public DbSet<EmailUsage> EmailUsages { get; set; }
        public DbSet<NotAvailableTime> NotAvailableTimes { get; set; }
        public DbSet<ShopPicture> ShopPictures { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<OpeningHours> OpeningHours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to be lowercase
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Business>().ToTable("businesses");
            modelBuilder.Entity<Appointment>().ToTable("appointments");
            modelBuilder.Entity<Service>().ToTable("services");
            modelBuilder.Entity<ServiceCategory>().ToTable("service_categories");
            modelBuilder.Entity<Staff>().ToTable("staffs");
            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<AppointmentServiceStaffMapping>().ToTable("appointment_services_staffs");
            modelBuilder.Entity<EmailUsage>().ToTable("email_usage");

            // Configure column names to be lowercase
            modelBuilder.Entity<User>().Property(u => u.UserId).HasColumnName("user_id");
            modelBuilder.Entity<User>().Property(u => u.Username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(u => u.Password).HasColumnName("password");
            modelBuilder.Entity<User>().Property(u => u.Email).HasColumnName("email");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasColumnName("updated_at");
            modelBuilder.Entity<User>().Property(u => u.Role).HasColumnName("role");
            modelBuilder.Entity<User>().Property(u => u.Phone).HasColumnName("phone");

            modelBuilder.Entity<Business>().Property(b => b.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<Business>().Property(b => b.Name).HasColumnName("name");
            modelBuilder.Entity<Business>().Property(b => b.Address).HasColumnName("address");
            modelBuilder.Entity<Business>().Property(b => b.Phone).HasColumnName("phone");
            modelBuilder.Entity<Business>().Property(b => b.Email).HasColumnName("email");
            modelBuilder.Entity<Business>().Property(b => b.UserID).HasColumnName("user_id");

            modelBuilder.Entity<Appointment>().Property(a => a.AppointmentId).HasColumnName("appointment_id");
            modelBuilder.Entity<Appointment>().Property(a => a.CustomerId).HasColumnName("customer_id");
            modelBuilder.Entity<Appointment>().Property(a => a.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<Appointment>().Property(a => a.AppointmentTime).HasColumnName("appointment_time");
            modelBuilder.Entity<Appointment>().Property(a => a.Duration).HasColumnName("duration");
            modelBuilder.Entity<Appointment>().Property(a => a.Status).HasColumnName("status");
            modelBuilder.Entity<Appointment>().Property(a => a.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Appointment>().Property(a => a.UpdatedAt).HasColumnName("updated_at");
            modelBuilder.Entity<Appointment>().Property(a => a.Comment).HasColumnName("comment");

            modelBuilder.Entity<ServiceCategory>().ToTable("service_categories");
            modelBuilder.Entity<ServiceCategory>().HasKey(sc => sc.CategoryId); // Explicitly define the primary key
            modelBuilder.Entity<ServiceCategory>().Property(sc => sc.CategoryId).HasColumnName("category_id");
            modelBuilder.Entity<ServiceCategory>().Property(sc => sc.Name).HasColumnName("name");

            modelBuilder.Entity<Service>().Property(s => s.ServiceId).HasColumnName("service_id");
            modelBuilder.Entity<Service>().Property(s => s.Name).HasColumnName("name");
            modelBuilder.Entity<Service>().Property(s => s.Description).HasColumnName("description");
            modelBuilder.Entity<Service>().Property(s => s.Duration).HasColumnName("duration");
            modelBuilder.Entity<Service>().Property(s => s.Price).HasColumnName("price");
            modelBuilder.Entity<Service>().Property(s => s.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<Service>().Property(s => s.CategoryId).HasColumnName("category_id");

            // Configure NotAvailableDate entity
            modelBuilder.Entity<NotAvailableDate>().ToTable("not_available_dates");
            modelBuilder.Entity<NotAvailableDate>().Property(nad => nad.NotAvailableDateId)
                .HasColumnName("not_available_date_id");
            modelBuilder.Entity<NotAvailableDate>().Property(nad => nad.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<NotAvailableDate>().Property(nad => nad.StaffId).HasColumnName("staff_id");
            modelBuilder.Entity<NotAvailableDate>().Property(nad => nad.StartDate).HasColumnName("start_date");
            modelBuilder.Entity<NotAvailableDate>().Property(nad => nad.EndDate).HasColumnName("end_date");
            modelBuilder.Entity<NotAvailableDate>().Property(nad => nad.Reason).HasColumnName("reason");

            modelBuilder.Entity<ShopPicture>().ToTable("shop_pictures");
            modelBuilder.Entity<ShopPicture>().Property(sp => sp.ShopPictureId).HasColumnName("shop_picture_id");
            modelBuilder.Entity<ShopPicture>().Property(sp => sp.S3ImageUrl).HasColumnName("s3_image_url");
            modelBuilder.Entity<ShopPicture>().Property(sp => sp.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<ShopPicture>().Property(sp => sp.BusinessId)
                .HasColumnName("business_id"); // Rename to business_id

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Category)
                .WithMany(sc => sc.Services)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Staff>().Property(s => s.StaffId).HasColumnName("staff_id");
            modelBuilder.Entity<Staff>().Property(s => s.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<Staff>().Property(s => s.Name).HasColumnName("name");
            modelBuilder.Entity<Staff>().Property(s => s.Email).HasColumnName("email");
            modelBuilder.Entity<Staff>().Property(s => s.Phone).HasColumnName("phone");
            modelBuilder.Entity<Staff>().Property(s => s.Password).HasColumnName("password");

            modelBuilder.Entity<Customer>().Property(c => c.CustomerId).HasColumnName("customer_id");
            modelBuilder.Entity<Customer>().Property(c => c.Name).HasColumnName("name");
            modelBuilder.Entity<Customer>().Property(c => c.Email).HasColumnName("email");
            modelBuilder.Entity<Customer>().Property(c => c.Phone).HasColumnName("phone");

            // Configure the relationship between Customer and Business
            modelBuilder.Entity<Customer>().Property(c => c.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Business)
                .WithMany(b => b.Customers)
                .HasForeignKey(c => c.BusinessId);

            modelBuilder.Entity<AppointmentServiceStaffMapping>().Property(assm => assm.AppointmentId)
                .HasColumnName("appointment_id");
            modelBuilder.Entity<AppointmentServiceStaffMapping>().Property(assm => assm.ServiceId)
                .HasColumnName("service_id");
            modelBuilder.Entity<AppointmentServiceStaffMapping>().Property(assm => assm.StaffId)
                .HasColumnName("staff_id");

            // Add configuration for UserRefreshToken table
            modelBuilder.Entity<UserRefreshToken>().ToTable("user_refresh_tokens");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.Id).HasColumnName("id");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.UserId).HasColumnName("user_id");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.Token).HasColumnName("token");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.ExpiryTime).HasColumnName("expiry_time");

            // Configure EmailUsage table
            modelBuilder.Entity<EmailUsage>().ToTable("email_usage");
            modelBuilder.Entity<EmailUsage>().Property(eu => eu.EmailUsageId).HasColumnName("email_usage_id");
            modelBuilder.Entity<EmailUsage>().Property(eu => eu.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<EmailUsage>().Property(eu => eu.Year).HasColumnName("year");
            modelBuilder.Entity<EmailUsage>().Property(eu => eu.Month).HasColumnName("month");
            modelBuilder.Entity<EmailUsage>().Property(eu => eu.EmailCount).HasColumnName("email_count");

            // Configuration for NotAvailableTime entity
            modelBuilder.Entity<NotAvailableTime>().ToTable("not_available_times");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.NotAvailableTimeId)
                .HasColumnName("not_available_time_id");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.StaffId).HasColumnName("staff_id");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.Date).HasColumnName("date");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.From).HasColumnName("from");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.To).HasColumnName("to");
            modelBuilder.Entity<NotAvailableTime>().Property(nat => nat.Reason).HasColumnName("reason");

            // Configure relationships
            modelBuilder.Entity<NotAvailableTime>()
                .HasOne(nat => nat.Business)
                .WithMany(b => b.NotAvailableTimes)
                .HasForeignKey(nat => nat.BusinessId);

            modelBuilder.Entity<NotAvailableTime>()
                .HasOne(nat => nat.Staff)
                .WithMany(s => s.NotAvailableTimes)
                .HasForeignKey(nat => nat.StaffId);


            // Configure Notification entity
            modelBuilder.Entity<Notification>().ToTable("notification_table");
            modelBuilder.Entity<Notification>().Property(n => n.NotificationId).HasColumnName("notification_id");
            modelBuilder.Entity<Notification>().Property(n => n.BusinessId).HasColumnName("business_id");
            modelBuilder.Entity<Notification>().Property(n => n.Message).HasColumnName("message");

            modelBuilder.Entity<Notification>().Property(n => n.NotificationType)
                .HasColumnName("notification_type")
                .HasConversion(
                    v => v.ToString(), // Convert Enum to string when saving
                    v => (NotificationType)Enum.Parse(typeof(NotificationType),
                        v) // Convert string to Enum when reading
                );
            modelBuilder.Entity<Notification>().Property(n => n.CreatedAt).HasColumnName("created_at");


            // Configure the OpeningHours table
            modelBuilder.Entity<OpeningHours>().ToTable("opening_hours"); // Specify the table name in lowercase

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.Id).HasColumnName("id");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.BusinessId).HasColumnName("business_id");

            // Map the TimeSpan properties to their corresponding columns
            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.MondayOpeningTime).HasColumnName("monday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.MondayClosingTime).HasColumnName("monday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.TuesdayOpeningTime).HasColumnName("tuesday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.TuesdayClosingTime).HasColumnName("tuesday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.WednesdayOpeningTime).HasColumnName("wednesday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.WednesdayClosingTime).HasColumnName("wednesday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.ThursdayOpeningTime).HasColumnName("thursday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.ThursdayClosingTime).HasColumnName("thursday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.FridayOpeningTime).HasColumnName("friday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.FridayClosingTime).HasColumnName("friday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.SaturdayOpeningTime).HasColumnName("saturday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.SaturdayClosingTime).HasColumnName("saturday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.SundayOpeningTime).HasColumnName("sunday_opening_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.SundayClosingTime).HasColumnName("sunday_closing_time");

            modelBuilder.Entity<OpeningHours>()
                .Property(oh => oh.MinimumAdvanceBookingHours).HasColumnName("minimum_advance_booking_hours");

            // Configure relationships
            modelBuilder.Entity<Business>()
                .HasOne(b => b.User)
                .WithMany(u => u.Businesses)
                .HasForeignKey(b => b.UserID);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CustomerId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Business)
                .WithMany(b => b.Appointments)
                .HasForeignKey(a => a.BusinessId);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Staffs)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Services)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<AppointmentServiceStaffMapping>()
                .HasKey(assm => new { assm.AppointmentId, assm.ServiceId, assm.StaffId });

            modelBuilder.Entity<AppointmentServiceStaffMapping>()
                .HasOne(assm => assm.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(assm => assm.AppointmentId);

            modelBuilder.Entity<AppointmentServiceStaffMapping>()
                .HasOne(assm => assm.Service)
                .WithMany(s => s.AppointmentServicesStaffs)
                .HasForeignKey(assm => assm.ServiceId);

            modelBuilder.Entity<AppointmentServiceStaffMapping>()
                .HasOne(assm => assm.Staff)
                .WithMany(s => s.AppointmentServicesStaffs)
                .HasForeignKey(assm => assm.StaffId);

            modelBuilder.Entity<NotAvailableDate>()
                .HasOne(nad => nad.Business)
                .WithMany(b => b.NotAvailableDates)
                .HasForeignKey(nad => nad.BusinessId);

            modelBuilder.Entity<NotAvailableDate>()
                .HasOne(nad => nad.Staff)
                .WithMany(s => s.NotAvailableDates)
                .HasForeignKey(nad => nad.StaffId);

            modelBuilder.Entity<EmailUsage>()
                .HasOne(eu => eu.Business)
                .WithMany(b => b.EmailUsages) // Assuming you want to navigate from Business to EmailUsage
                .HasForeignKey(eu => eu.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShopPicture>()
                .HasOne(sp => sp.Business)
                .WithMany(b => b.ShopPictures)
                .HasForeignKey(sp => sp.BusinessId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: delete pictures when a business is deleted


            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Business)
                .WithMany(b => b.Notifications)
                .HasForeignKey(n => n.BusinessId);

            modelBuilder.Entity<OpeningHours>()
                .HasOne<Business>()
                .WithMany(b => b.OpeningHours)
                .HasForeignKey(oh => oh.BusinessId);

            // Add indexes to improve performance
            modelBuilder.Entity<Business>()
                .HasIndex(b => b.UserID);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.CustomerId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.BusinessId);

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.BusinessId);

            modelBuilder.Entity<Service>()
                .HasIndex(s => s.BusinessId);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.BusinessId);

            modelBuilder.Entity<NotAvailableDate>()
                .HasIndex(nad => nad.BusinessId);

            modelBuilder.Entity<NotAvailableDate>()
                .HasIndex(nad => nad.StaffId);

            modelBuilder.Entity<Business>()
                .HasIndex(b => b.UserID);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.CustomerId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.BusinessId);

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.BusinessId);

            modelBuilder.Entity<Service>()
                .HasIndex(s => s.BusinessId);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.BusinessId);

            // Indexes
            modelBuilder.Entity<UserRefreshToken>()
                .HasIndex(urt => urt.Token)
                .IsUnique();
            modelBuilder.Entity<EmailUsage>()
                .HasIndex(eu => new { eu.BusinessId, eu.Year, eu.Month })
                .IsUnique(); // Ensure uniqueness for each Business per month

            modelBuilder.Entity<ShopPicture>()
                .HasIndex(sp => sp.BusinessId)
                .HasDatabaseName("IX_ShopPictures_BusinessId");

            // Relationship with Business
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Business)
                .WithMany(b => b.Notifications)
                .HasForeignKey(n => n.BusinessId);

            // Indexes to optimize performance
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.BusinessId)
                .HasDatabaseName("IX_Notification_BusinessId");

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt)
                .HasDatabaseName("IX_Notification_CreatedAt");

            // Add indexes for performance
            modelBuilder.Entity<NotAvailableTime>()
                .HasIndex(nat => nat.StaffId)
                .HasDatabaseName("IX_NotAvailableTime_StaffId");

            modelBuilder.Entity<NotAvailableTime>()
                .HasIndex(nat => nat.BusinessId)
                .HasDatabaseName("IX_NotAvailableTime_BusinessId");

            modelBuilder.Entity<NotAvailableTime>()
                .HasIndex(nat => new { nat.Date, nat.From, nat.To })
                .HasDatabaseName("IX_NotAvailableTime_DateRange");

            modelBuilder.Entity<OpeningHours>()
                .HasIndex(oh => oh.BusinessId).HasDatabaseName("IX_OpeningHours_BusinessId");
        }
    }
}