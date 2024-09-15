using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Services.Interfaces.ServicesService
{
    public interface IServicesService
    {
        Task<IEnumerable<ServiceDto?>> GetAllServicesAsync(); // Change return type to include category name in ServiceDto
        Task<ServiceDto?> GetServiceByIdAsync(int id); // Change return type to include category name in ServiceDto
        Task<IEnumerable<ServiceDto?>> GetAllServiceByBusinessIdAsync(int businessId); // Change return type to include category name in ServiceDto
        Task AddServiceAsync(ServiceDto? serviceDto, int businessId, string userId, string userRole);
        Task AddListServicesAsync(List<ServiceDto>? servicesDto, int businessId, string userId, string userRole);
        Task UpdateServiceAsync(int businessId, int serviceId, ServiceDto? serviceDto, string userId);
        Task DeleteServiceAsync(int businessId, int serviceId);
    }
}