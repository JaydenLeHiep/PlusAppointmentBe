using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.ServicesRepo;

public interface IServicesRepository
{
    Task<IEnumerable<Service?>> GetAllAsync();
    Task<Service?> GetByIdAsync(int id);
    
    Task<IEnumerable<Service?>> GetAllByBusinessIdAsync(int businessId);
    Task<Service?> GetByBusinessIdServiceIdAsync(int businessId, int serviceId);

    public Task AddServiceAsync(Service? service);
    public Task AddListServicesAsync(IEnumerable<Service?> services);
    Task UpdateAsync(Service service);
    Task DeleteAsync(int businessId, int serviceId);
    
}