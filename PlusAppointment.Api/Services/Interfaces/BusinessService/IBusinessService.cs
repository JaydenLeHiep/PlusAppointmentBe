using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlusAppointment.Models.Classes.Business;

namespace PlusAppointment.Services.Interfaces.BusinessService
{
    public interface IBusinessService
    {
        Task<IEnumerable<Business?>> GetAllBusinessesAsync();
        Task<Business?> GetBusinessByIdAsync(int id);
        Task AddBusinessAsync(BusinessDto businessDto, int userId);
        Task UpdateBusinessAsync(int businessId, BusinessDto businessDto, int currentUserId, string userRole);
        Task DeleteBusinessAsync(int businessId, int currentUserId, string userRole);
        Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId);
        Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId);
        Task<IEnumerable<Business?>> GetAllBusinessesByUserIdAsync(int userId);
        Task<Business?> GetBusinessByNameAsync(string businessName);
    }
}