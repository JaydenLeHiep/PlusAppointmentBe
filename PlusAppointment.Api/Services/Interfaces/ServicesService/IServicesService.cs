using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Services;

namespace PlusAppointment.Services.Interfaces.ServicesService
{
    public interface IServicesService
    {
        Task<IEnumerable<ServiceDto?>> GetAllServicesAsync(); // Change return type to include category name in ServiceDto
        Task<ServiceDto?> GetServiceByIdAsync(int id); // Change return type to include category name in ServiceDto
        Task<IEnumerable<ServiceDto?>> GetAllServiceByBusinessIdAsync(int businessId); // Change return type to include category name in ServiceDto
        Task AddServiceAsync(Service? service);
        Task AddListServicesAsync(List<Service>? services);
        Task UpdateServiceAsync(Service? service);
        Task<Service?> GetByBusinessIdServiceIdAsync(int businessId, int serviceId);
        Task DeleteServiceAsync(int businessId, int serviceId);
    }
}