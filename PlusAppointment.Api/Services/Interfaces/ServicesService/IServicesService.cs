using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Services.Interfaces.ServicesService
{
    public interface IServicesService
    {
        Task<IEnumerable<Service?>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<IEnumerable<Service?>> GetAllServiceByBusinessIdAsync(int businessId);
        Task AddServiceAsync(ServiceDto? serviceDto, int businessId, string userId, string userRole);
        Task AddListServicesAsync(ServicesDto? servicesDto, int businessId, string userId, string userRole);
        Task UpdateServiceAsync(int businessId, int serviceId, ServiceDto? serviceDto, string userId);
        Task DeleteServiceAsync(int businessId, int serviceId);
    }
}