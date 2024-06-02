using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using WebApplication1.Data;
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
        var staff = await _context.Staffs.FindAsync(id);
        if (staff == null)
        {
            throw new KeyNotFoundException($"Staff with ID {id} not found");
        }
        return staff;
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

    public async Task AddListStaffsAsync(IEnumerable<Staff> staffs)
    {
        var enumerable = staffs.ToList();
        if (staffs == null || !enumerable.Any())
        {
            throw new ArgumentException("Staffs collection cannot be null or empty", nameof(staffs));
        }

        var staffList = enumerable.ToList();
        var businessId = staffList.First().BusinessId;
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
        {
            throw new Exception("Business not found");
        }

        await _context.Staffs.AddRangeAsync(staffList);
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
    
    public async Task<Staff> GetByEmailAsync(string email)
    {
        var staff = await _context.Staffs.SingleOrDefaultAsync(s => s.Email == email);
        if (staff == null)
        {
            throw new KeyNotFoundException($"Staff with email {email} not found");
        }
        return staff;
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Staffs.AnyAsync(s => s.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        return await _context.Staffs.AnyAsync(s => s.Phone == phone);
    }
}