using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Staff;

namespace PlusAppointment.Services.Interfaces.StaffService;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllStaffsAsync();
    Task<Staff> GetStaffIdAsync(int id);
    Task<IEnumerable<StaffRetrieveDto?>> GetAllStaffByBusinessIdAsync(int businessId);
    Task AddStaffAsync(Staff? staff, int businessId);
    public  Task AddListStaffsAsync(IEnumerable<Staff?> staff);
    Task<Staff?> GetByBusinessIdStaffIdAsync(int businessId, int staffId);
    Task UpdateStaffAsync(Staff staff);
    Task DeleteStaffAsync(int businessId, int staffId);
    
    Task<string> LoginAsync(string email, string password);
}