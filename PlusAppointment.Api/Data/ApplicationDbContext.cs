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
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AppointmentServiceStaffMapping> AppointmentServiceStaffs { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to be lowercase
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Business>().ToTable("businesses");
            modelBuilder.Entity<Appointment>().ToTable("appointments");
            modelBuilder.Entity<Service>().ToTable("services");
            modelBuilder.Entity<Staff>().ToTable("staffs");
            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<AppointmentServiceStaffMapping>().ToTable("appointment_services_staffs");

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

            modelBuilder.Entity<Service>().Property(s => s.ServiceId).HasColumnName("service_id");
            modelBuilder.Entity<Service>().Property(s => s.Name).HasColumnName("name");
            modelBuilder.Entity<Service>().Property(s => s.Description).HasColumnName("description");
            modelBuilder.Entity<Service>().Property(s => s.Duration).HasColumnName("duration");
            modelBuilder.Entity<Service>().Property(s => s.Price).HasColumnName("price");
            modelBuilder.Entity<Service>().Property(s => s.BusinessId).HasColumnName("business_id");

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

            modelBuilder.Entity<AppointmentServiceStaffMapping>().Property(assm => assm.AppointmentId).HasColumnName("appointment_id");
            modelBuilder.Entity<AppointmentServiceStaffMapping>().Property(assm => assm.ServiceId).HasColumnName("service_id");
            modelBuilder.Entity<AppointmentServiceStaffMapping>().Property(assm => assm.StaffId).HasColumnName("staff_id");

            // Add configuration for UserRefreshToken table
            modelBuilder.Entity<UserRefreshToken>().ToTable("user_refresh_tokens");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.Id).HasColumnName("id");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.UserId).HasColumnName("user_id");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.Token).HasColumnName("token");
            modelBuilder.Entity<UserRefreshToken>().Property(urt => urt.ExpiryTime).HasColumnName("expiry_time");

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
            
            // Indexes
            modelBuilder.Entity<UserRefreshToken>()
                .HasIndex(urt => urt.Token)
                .IsUnique();
        }
    }
}
