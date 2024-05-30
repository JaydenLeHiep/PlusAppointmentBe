using PlusAppointment.Models.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces.StaffService;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllStaffsAsync();
    Task<Staff> GetStaffIdAsync(int id);
    Task AddStaffAsync(StaffDto staffDto);
    Task AddListStaffsAsync(IEnumerable<StaffDto> staffDtos);
    Task UpdateStaffAsync(int id, StaffDto staffDto);
    Task DeleteStaffAsync(int id);
    
    Task<string> LoginAsync(string email, string password);
}