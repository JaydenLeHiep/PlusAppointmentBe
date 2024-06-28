using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace WebApplication1.Services.Interfaces.StaffService;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllStaffsAsync();
    Task<Staff> GetStaffIdAsync(int id);
    Task<IEnumerable<Staff?>> GetAllStaffByBusinessIdAsync(int businessId);
    Task AddStaffAsync(StaffDto staffDto);
    public  Task AddListStaffsAsync(IEnumerable<StaffDto> staffDtos, int businessId);
    Task UpdateStaffAsync(int id, StaffDto staffDto);
    Task DeleteStaffAsync(int id);
    
    Task<string> LoginAsync(string email, string password);
}