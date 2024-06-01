using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.ServicesRepo;
using WebApplication1.Services.Interfaces.ServicesService;

namespace WebApplication1.Services.Implematations.ServicesService;

public class ServicesService: IServicesService
{
    private readonly IServicesRepository _servicesRepository;

    public ServicesService(IServicesRepository servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }
    
    public async Task<IEnumerable<Service?>> GetAllServicesAsync()
    {
        return await _servicesRepository.GetAllAsync();
    }

    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        return await _servicesRepository.GetByIdAsync(id);
    }

    public async Task AddServiceAsync(Service? service, int businessId)
    {
        await _servicesRepository.AddServiceAsync(service, businessId);
    }

    public async Task AddListServicesAsync(IEnumerable<Service?> services,int businessId)
    {
        await _servicesRepository.AddListServicesAsync(services, businessId);
    }

    public async Task UpdateServiceAsync(Service? service)
    {
        await _servicesRepository.UpdateAsync(service);
    }

    public async Task DeleteServiceAsync(int id)
    {
        await _servicesRepository.DeleteAsync(id);
    }
}