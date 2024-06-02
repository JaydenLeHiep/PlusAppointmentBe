using PlusAppointment.Models.Classes;

namespace WebApplication1.Services.Interfaces.ServicesService;

public interface IServicesService
{
    Task<IEnumerable<Service?>> GetAllServicesAsync();
    Task<Service?> GetServiceByIdAsync(int id);
    Task AddServiceAsync(Service? service, int businessId);
    Task AddListServicesAsync(IEnumerable<Service?> services, int businessId);
    Task UpdateServiceAsync(Service? service);
    Task DeleteServiceAsync(int id);
    

}