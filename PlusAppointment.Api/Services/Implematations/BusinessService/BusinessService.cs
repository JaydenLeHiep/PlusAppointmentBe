using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.BusinessRepo;
using WebApplication1.Services.Interfaces.BusinessService;

namespace WebApplication1.Services.Implematations.BusinessService;

public class BusinessService: IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<IEnumerable<Business>> GetAllBusinessesAsync()
    {
        return await _businessRepository.GetAllAsync();
    }

    public async Task<Business> GetBusinessByIdAsync(int id)
    {
        return await _businessRepository.GetByIdAsync(id);
    }

    public async Task AddBusinessAsync(Business business)
    {
        await _businessRepository.AddAsync(business);
    }

    public async Task UpdateBusinessAsync(Business business)
    {
        await _businessRepository.UpdateAsync(business);
    }

    public async Task DeleteBusinessAsync(int id)
    {
        await _businessRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Service>> GetServicesByBusinessIdAsync(int businessId)
    {
        return await _businessRepository.GetServicesByBusinessIdAsync(businessId);
    }

    public async Task<IEnumerable<Staff>> GetStaffByBusinessIdAsync(int businessId)
    {
        return await _businessRepository.GetStaffByBusinessIdAsync(businessId);
    }
}