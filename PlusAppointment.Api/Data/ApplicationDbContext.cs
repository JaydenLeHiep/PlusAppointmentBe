using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

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
        public DbSet<Staff> Staffs { get; set; }  // Updated from Staff to Staffs
        public DbSet<Customer> Customers { get; set; }
        public DbSet<BusinessServices> BusinessServices { get; set; }

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
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany()
                .HasForeignKey(a => a.StaffId);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Staffs)  // Updated from b.Staff to b.Staffs
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<Service>()
                .HasMany(s => s.Businesses)
                .WithMany(b => b.Services)
                .UsingEntity<BusinessServices>(
                    j => j
                        .HasOne(bs => bs.Business)
                        .WithMany(b => b.BusinessServices)
                        .HasForeignKey(bs => bs.BusinessId),
                    j => j
                        .HasOne(bs => bs.Service)
                        .WithMany(s => s.BusinessServices)
                        .HasForeignKey(bs => bs.ServiceId),
                    j => { j.HasKey(bs => new { bs.BusinessId, bs.ServiceId }); });

            // Add indexes to improve performance
            modelBuilder.Entity<Business>()
                .HasIndex(b => b.UserID);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.CustomerId);
            
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.BusinessId);
            
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.ServiceId);
            
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.StaffId);

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.BusinessId);

            modelBuilder.Entity<BusinessServices>()
                .HasIndex(bs => new { bs.BusinessId, bs.ServiceId });
        }
    }
}
