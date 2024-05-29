using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces.StaffRepo;

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff> GetByIdAsync(int id);
    public Task AddStaffAsync(Staff staff, int businessId);
    public Task AddListStaffsAsync(IEnumerable<Staff> staffs, int businessId);
    Task UpdateAsync(Staff staff);
    Task DeleteAsync(int id);
}