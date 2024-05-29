using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.StaffRepo;

namespace WebApplication1.Repositories.Implementation.StaffRepo;

public class StaffRepository: IStaffRepository
{
    private readonly ApplicationDbContext _context;

    public StaffRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _context.Staffs.ToListAsync();
    }

    public async Task<Staff> GetByIdAsync(int id)
    {
        return await _context.Staffs.FindAsync(id);
    }

    public async Task AddStaffAsync(Staff staff, int businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
        {
            throw new Exception("Business not found");
        }

        staff.BusinessId = businessId;
        await _context.Staffs.AddAsync(staff);
        await _context.SaveChangesAsync();
    }

    public async Task AddListStaffsAsync(IEnumerable<Staff> staffs, int businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
        {
            throw new Exception("Business not found");
        }

        foreach (var staff in staffs)
        {
            staff.BusinessId = businessId;
        }

        await _context.Staffs.AddRangeAsync(staffs);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(Staff staff)
    {
        _context.Staffs.Update(staff);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await _context.Staffs.FindAsync(id);
        if (staff != null)
        {
            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
        }
    }
}