using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Classes.Business;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;

namespace PlusAppointment.Repositories.Implementation.BusinessRepo
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BusinessRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Business?>> GetAllAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var businesses = await context.Businesses.ToListAsync();
                return businesses;
            }
        }

        public async Task<Business?> GetByIdAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(id);
                if (business == null)
                {
                    return null;
                }
                
                return business;
            }
        }

        public async Task<Business?> GetByNameAsync(string businessName)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FirstOrDefaultAsync(b => b.Name.ToLower() == businessName.ToLower());
                if (business == null)
                {
                    throw new KeyNotFoundException($"Business with name {businessName} not found");
                }
                return business;
            }
        }

        public async Task AddAsync(Business business)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                await context.Businesses.AddAsync(business);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Business business)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Businesses.Update(business);
                await context.SaveChangesAsync();
            }
            
        }

        public async Task DeleteAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(id);
                if (business != null)
                {
                    context.Businesses.Remove(business);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var services = await context.Services
                    .Where(s => s.BusinessId == businessId)
                    .ToListAsync();
                return services;
            }
        }

        public async Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs.Where(s => s.BusinessId == businessId).ToListAsync();
                return staff;
            }
        }

        public async Task<IEnumerable<Business?>> GetAllByUserIdAsync(int userId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var businesses = await context.Businesses.Where(b => b.UserID == userId).ToListAsync();
                return businesses;
            }
        }
        
        public async Task<decimal?> GetBirthdayDiscountPercentageAsync(int businessId)
        {
            using var context = _contextFactory.CreateDbContext();
            var business = await context.Businesses
                .Where(b => b.BusinessId == businessId)
                .Select(b => b.BirthdayDiscountPercentage)
                .FirstOrDefaultAsync();

            return business;
        }
    }
}
