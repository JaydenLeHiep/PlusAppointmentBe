using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Services.Interfaces.BusinessService;

using PlusAppointment.Models.Enums;

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

        public async Task AddBusinessAsync(BusinessDto businessDto, int userId)
        {
            var business = new Business
            (
                name: businessDto.Name,
                address: businessDto.Address,
                phone: businessDto.Phone,
                email: businessDto.Email,
                userID: userId
            );
            await _businessRepository.AddAsync(business);
        }

        public async Task UpdateBusinessAsync(int businessId, BusinessDto businessDto, int currentUserId, string userRole)
        {
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
            {
                throw new Exception("Business not found.");
            }

            if (business.UserID != currentUserId && userRole != Role.Admin.ToString())
            {
                throw new Exception("You are not authorized to update this business.");
            }

            if (!string.IsNullOrEmpty(businessDto.Name))
            {
                business.Name = businessDto.Name;
            }

            if (!string.IsNullOrEmpty(businessDto.Address))
            {
                business.Address = businessDto.Address;
            }

            if (!string.IsNullOrEmpty(businessDto.Phone))
            {
                business.Phone = businessDto.Phone;
            }

            if (!string.IsNullOrEmpty(businessDto.Email))
            {
                business.Email = businessDto.Email;
            }

            await _businessRepository.UpdateAsync(business);
        }

        public async Task DeleteBusinessAsync(int businessId, int currentUserId, string userRole)
        {
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
            {
                throw new Exception("Business not found.");
            }

            if (business.UserID != currentUserId && userRole != Role.Admin.ToString())
            {
                throw new Exception("You are not authorized to delete this business.");
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
