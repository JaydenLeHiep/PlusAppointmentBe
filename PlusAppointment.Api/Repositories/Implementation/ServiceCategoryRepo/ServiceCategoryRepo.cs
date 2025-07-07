using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;


namespace PlusAppointment.Repositories.Implementation.ServiceCategoryRepo
{
    public class ServiceCategoryRepo : IServiceCategoryRepo
    {
        private readonly ApplicationDbContext _context;

        public ServiceCategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceCategory?> GetServiceCategoryByIdAsync(int id)
        {
            var serviceCategory = await _context.ServiceCategories.FindAsync(id);
            if (serviceCategory == null)
            {
                throw new KeyNotFoundException($"ServiceCategory with ID {id} not found");
            }
            return serviceCategory;
        }

        public async Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync()
        {
            var serviceCategories = await _context.ServiceCategories.ToListAsync();

            return serviceCategories;
        }

        public async Task AddServiceCategoryAsync(ServiceCategory serviceCategory)
        {
            _context.ServiceCategories.Add(serviceCategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateServiceCategoryAsync(ServiceCategory serviceCategory)
        {
            _context.ServiceCategories.Update(serviceCategory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteServiceCategoryAsync(int id)
        {
            var serviceCategory = await _context.ServiceCategories.FindAsync(id);
            if (serviceCategory != null)
            {
                _context.ServiceCategories.Remove(serviceCategory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
