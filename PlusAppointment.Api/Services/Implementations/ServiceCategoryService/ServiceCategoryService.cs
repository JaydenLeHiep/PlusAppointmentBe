using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;
using PlusAppointment.Services.Interfaces.ServiceCategoryService;

namespace PlusAppointment.Services.Implementations.ServiceCategoryService;

public class ServiceCategoryService : IServiceCategoryService
{
    private readonly IServiceCategoryRepo _serviceCategoryRepo;

    public ServiceCategoryService(IServiceCategoryRepo serviceCategoryRepo)
    {
        _serviceCategoryRepo = serviceCategoryRepo;
    }

    public async Task<ServiceCategory?> GetServiceCategoryByIdAsync(int id)
    {
        return await _serviceCategoryRepo.GetServiceCategoryByIdAsync(id);
    }

    public async Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync()
    {
        return await _serviceCategoryRepo.GetAllServiceCategoriesAsync();
    }

    public async Task AddServiceCategoryAsync(ServiceCategory serviceCategory)
    {
        await _serviceCategoryRepo.AddServiceCategoryAsync(serviceCategory);
    }

    public async Task UpdateServiceCategoryAsync(ServiceCategory serviceCategory)
    {
        await _serviceCategoryRepo.UpdateServiceCategoryAsync(serviceCategory);
    }

    public async Task DeleteServiceCategoryAsync(int id)
    {
        await _serviceCategoryRepo.DeleteServiceCategoryAsync(id);
    }
}