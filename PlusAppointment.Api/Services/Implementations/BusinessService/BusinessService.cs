using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Classes.Business;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Services.Interfaces.BusinessService;


namespace PlusAppointment.Services.Implementations.BusinessService
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;

        public BusinessService(IBusinessRepository businessRepository)
        {
            _businessRepository = businessRepository;
        }

        public async Task<IEnumerable<Business?>> GetAllBusinessesAsync()
        {
            return await _businessRepository.GetAllAsync();
        }

        public async Task<Business?> GetBusinessByIdAsync(int id)
        {
            return await _businessRepository.GetByIdAsync(id);
        }
        public async Task<Business?> GetBusinessByNameAsync(string businessName)
        {
            return await _businessRepository.GetByNameAsync(businessName);
        }

        public async Task AddBusinessAsync(Business business)
        {
            await _businessRepository.AddAsync(business);
        }

        public async Task UpdateBusinessAsync(int businessId, Business business)
        {
            var existingBusiness = await _businessRepository.GetByIdAsync(businessId);
            if (existingBusiness == null)
            {
                throw new KeyNotFoundException("Business not found.");
            }

            // Update the entity in the repository
            await _businessRepository.UpdateAsync(business);
        }


        public async Task DeleteBusinessAsync(int businessId)
        {
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
            {
                throw new KeyNotFoundException("Business not found.");
            }
            

            await _businessRepository.DeleteAsync(businessId);
        }

        public async Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId)
        {
            return await _businessRepository.GetServicesByBusinessIdAsync(businessId);
        }

        public async Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId)
        {
            return await _businessRepository.GetStaffByBusinessIdAsync(businessId);
        }

        public async Task<IEnumerable<Business?>> GetAllBusinessesByUserIdAsync(int userId)
        {
            return await _businessRepository.GetAllByUserIdAsync(userId);
        }
    }
}
