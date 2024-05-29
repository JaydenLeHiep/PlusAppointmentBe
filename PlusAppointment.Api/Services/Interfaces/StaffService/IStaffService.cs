using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces.StaffService;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllStaffsAsync();
    Task<Staff> GetStaffIdAsync(int id);
    Task AddStaffAsync(Staff staff, int businessId);
    Task AddListStaffsAsync(IEnumerable<Staff> staffs, int businessId);
    Task UpdateStaffAsync(Staff staff);
    Task DeleteStaffAsync(int id);
}