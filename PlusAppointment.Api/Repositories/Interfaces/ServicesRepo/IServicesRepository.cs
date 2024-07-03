using PlusAppointment.Models.Classes;

namespace WebApplication1.Repositories.Interfaces.ServicesRepo;

public interface IServicesRepository
{
    Task<IEnumerable<Service?>> GetAllAsync();
    Task<Service?> GetByIdAsync(int id);
    
    Task<IEnumerable<Service?>> GetAllByBusinessIdAsync(int businessId);
    public Task AddServiceAsync(Service? service, int businessId);
    public Task AddListServicesAsync(IEnumerable<Service?> services, int businessId);
    Task UpdateAsync(Service service);
    Task DeleteAsync(int businessId, int serviceId);
    
}