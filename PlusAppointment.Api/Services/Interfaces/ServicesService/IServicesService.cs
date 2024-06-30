using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace WebApplication1.Services.Interfaces.ServicesService
{
    public interface IServicesService
    {
        Task<IEnumerable<Service?>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task AddServiceAsync(ServiceDto? serviceDto, int businessId, string userId, string userRole);
        Task AddListServicesAsync(ServicesDto? servicesDto, int businessId, string userId, string userRole);
        Task UpdateServiceAsync(int id, ServiceDto? serviceDto, string userId);
        Task DeleteServiceAsync(int businessId, int serviceId);
    }
}