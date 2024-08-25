using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;

public interface IServiceCategoryRepo
{
    Task<ServiceCategory?> GetServiceCategoryByIdAsync(int id);
    Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync();
    Task AddServiceCategoryAsync(ServiceCategory serviceCategory);
    Task UpdateServiceCategoryAsync(ServiceCategory serviceCategory);
    Task DeleteServiceCategoryAsync(int id);
}