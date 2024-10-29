using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Staff;

namespace PlusAppointment.Services.Interfaces.StaffService;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllStaffsAsync();
    Task<Staff> GetStaffIdAsync(int id);
    Task<IEnumerable<StaffRetrieveDto?>> GetAllStaffByBusinessIdAsync(int businessId);
    Task AddStaffAsync(StaffDto? staffDto, int businessId);
    public  Task AddListStaffsAsync(IEnumerable<StaffDto?> staffDtos, int businessId);
    Task UpdateStaffAsync(int businessId, int staffId, StaffDto staffDto);
    Task DeleteStaffAsync(int businessId, int staffId);
    
    Task<string> LoginAsync(string email, string password);
}