using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Classes.Business;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Models.Classes.Emails;

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
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<EmailContent> EmailContents { get; set; }
        public DbSet<DiscountTier> DiscountTiers { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}