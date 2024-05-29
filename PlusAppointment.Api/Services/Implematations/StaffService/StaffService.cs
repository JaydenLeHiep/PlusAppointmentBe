using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.StaffRepo;
using WebApplication1.Services.Interfaces.StaffService;

namespace WebApplication1.Services.Implematations.StaffService;

public class StaffService: IStaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }
    
    public async Task<IEnumerable<Staff>> GetAllStaffsAsync()
    {
        return await _staffRepository.GetAllAsync();
    }

    public async Task<Staff> GetStaffIdAsync(int id)
    {
        return await _staffRepository.GetByIdAsync(id);
    }

    public async Task AddStaffAsync(Staff staff, int businessId)
    {
        await _staffRepository.AddStaffAsync(staff, businessId);
    }

    public async Task AddListStaffsAsync(IEnumerable<Staff> staffs, int businessId)
    {
        await _staffRepository.AddListStaffsAsync(staffs, businessId);
    }

    public async Task UpdateStaffAsync(Staff staff)
    {
        await _staffRepository.UpdateAsync(staff);
    }

    public async Task DeleteStaffAsync(int id)
    {
        await _staffRepository.DeleteAsync(id);
    }
}