using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.ServiceCategoryService;

public interface IServiceCategoryService
{
    Task<ServiceCategory?> GetServiceCategoryByIdAsync(int id);
    Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync();
    Task AddServiceCategoryAsync(ServiceCategory serviceCategory);
    Task UpdateServiceCategoryAsync(ServiceCategory serviceCategory);
    Task DeleteServiceCategoryAsync(int id);
}