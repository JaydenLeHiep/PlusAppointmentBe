using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.BusinessRepo;

namespace WebApplication1.Repositories.Implementation.BusinessRepo;

public class BusinessRepository: IBusinessRepository
{
    private readonly ApplicationDbContext _context;

    public BusinessRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        return await _context.Businesses.ToListAsync();
    }

    public async Task<Business> GetByIdAsync(int id)
    {
        return await _context.Businesses.FindAsync(id);
    }

    public async Task AddAsync(Business business)
    {
        await _context.Businesses.AddAsync(business);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Business business)
    {
        _context.Businesses.Update(business);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business != null)
        {
            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Service>> GetServicesByBusinessIdAsync(int businessId)
    {
        return await _context.BusinessServices
            .Where(bs => bs.BusinessId == businessId)
            .Select(bs => bs.Service)
            .ToListAsync();
    }

    public async Task<IEnumerable<Staff>> GetStaffByBusinessIdAsync(int businessId)
    {
        return await _context.Staffs.Where(s => s.BusinessId == businessId).ToListAsync();
    }
}