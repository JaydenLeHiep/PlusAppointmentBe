using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;

namespace WebApplication1.Data
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
        public DbSet<AppointmentServiceMapping> AppointmentServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.StaffId);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Staffs)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Services)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<AppointmentServiceMapping>()
                .HasKey(apptService => new { apptService.AppointmentId, apptService.ServiceId });

            modelBuilder.Entity<AppointmentServiceMapping>()
                .HasOne(apptService => apptService.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(apptService => apptService.AppointmentId);

            modelBuilder.Entity<AppointmentServiceMapping>()
                .HasOne(apptService => apptService.Service)
                .WithMany(s => s.AppointmentServices)
                .HasForeignKey(apptService => apptService.ServiceId);

            // Add indexes to improve performance
            modelBuilder.Entity<Business>()
                .HasIndex(b => b.UserID);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.CustomerId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.BusinessId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.StaffId);

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.BusinessId);

            modelBuilder.Entity<Service>()
                .HasIndex(s => s.BusinessId);
        }
    }
}
