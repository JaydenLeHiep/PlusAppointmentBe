using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlusAppointment.Models.Classes.Business;
using PlusAppointment.Models.DTOs.Businesses;

namespace PlusAppointment.Services.Interfaces.BusinessService
{
    public interface IBusinessService
    {
        Task<IEnumerable<Business?>> GetAllBusinessesAsync();
        Task<Business?> GetBusinessByIdAsync(int id);
        Task AddBusinessAsync(Business business);
        Task UpdateBusinessAsync(int businessId, Business business);
        Task DeleteBusinessAsync(int businessId);
        Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId);
        Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId);
        Task<IEnumerable<Business?>> GetAllBusinessesByUserIdAsync(int userId);
        Task<Business?> GetBusinessByNameAsync(string businessName);
    }
}