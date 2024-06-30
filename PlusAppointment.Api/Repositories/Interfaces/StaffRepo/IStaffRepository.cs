using PlusAppointment.Models.Classes;

namespace WebApplication1.Repositories.Interfaces.StaffRepo;

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    
    Task<IEnumerable<Staff?>> GetAllByBusinessIdAsync(int businessId);
    Task<Staff> GetByIdAsync(int id);
    public Task AddStaffAsync(Staff staff, int businessId);
    public  Task AddListStaffsAsync(IEnumerable<Staff> staffs);
    Task UpdateAsync(Staff staff);
    Task DeleteAsync(int businessId, int staffId);
    Task<Staff> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
}