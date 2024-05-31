using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.ServicesRepo;

namespace WebApplication1.Repositories.Implementation.ServicesRepo;

public class ServicesRepository: IServicesRepository
{
    private readonly ApplicationDbContext _context;

    public ServicesRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        return await _context.Services.ToListAsync();
    }

    public async Task<Service> GetByIdAsync(int id)
    {
        return await _context.Services.FindAsync(id);
    }
    
    // add one service in a time (for the business owner)
    public async Task AddServiceAsync(Service service, int businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
        {
            throw new Exception("Business not found");
        }

        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        //business.BusinessServices.Add(new BusinessServices { BusinessId = businessId, ServiceId = service.ServiceId });
        await _context.SaveChangesAsync();
    }

    public async Task AddListServicesAsync(IEnumerable<Service> services, int businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
        {
            throw new Exception("Business not found");
        }

        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();


        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }
}