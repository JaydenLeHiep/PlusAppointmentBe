using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.StaffRepo;

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    
    Task<IEnumerable<Staff?>> GetAllByBusinessIdAsync(int businessId);
    Task<Staff?> GetByBusinessIdStaffIdAsync(int businessId, int staffId);
    Task<Staff> GetByIdAsync(int id);
    public Task AddStaffAsync(Staff staff, int businessId);
    public Task AddListStaffsAsync(Staff?[] staffs);
    Task UpdateAsync(Staff staff);
    Task DeleteAsync(int businessId, int staffId);
    Task<Staff> GetByEmailAsync(string email);

}