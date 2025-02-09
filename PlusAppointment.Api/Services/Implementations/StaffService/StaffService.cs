using AutoMapper;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Staff;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Interfaces.StaffService;
using PlusAppointment.Utils.Hash;
using PlusAppointment.Utils.Jwt;

namespace PlusAppointment.Services.Implementations.StaffService
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IConfiguration _configuration;
        private readonly IHashUtility _hashUtility;
        private readonly IMapper _mapper;
        public StaffService(IStaffRepository staffRepository, IConfiguration configuration, IHashUtility hashUtility, IMapper mapper)
        {
            _staffRepository = staffRepository;
            _configuration = configuration;
            _hashUtility = hashUtility;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Staff>> GetAllStaffsAsync()
        {
            return await _staffRepository.GetAllAsync();
        }

        public async Task<Staff> GetStaffIdAsync(int id)
        {
            var staff = await _staffRepository.GetByIdAsync(id);


            return staff;
        }

        public async Task<IEnumerable<StaffRetrieveDto?>> GetAllStaffByBusinessIdAsync(int businessId)
        {
            var staffs = await _staffRepository.GetAllByBusinessIdAsync(businessId);
            return _mapper.Map<IEnumerable<StaffRetrieveDto>>(staffs);
        }

        public async Task AddStaffAsync(Staff? staff, int businessId)
        {
            if (staff != null) await _staffRepository.AddStaffAsync(staff, businessId);
        }

        public async Task AddListStaffsAsync(IEnumerable<Staff?> staffs)
        {
            var enumerable = staffs as Staff[] ?? staffs.ToArray();
            if (staffs == null || !enumerable.Any())
            {
                throw new ArgumentException("Staff collection cannot be empty.");
            }

            await _staffRepository.AddListStaffsAsync(enumerable);
        }

        public async Task<Staff?> GetByBusinessIdStaffIdAsync(int businessId, int staffId)
        {
            return await _staffRepository.GetByBusinessIdStaffIdAsync(businessId, staffId);
        }

        public async Task UpdateStaffAsync(Staff staff)
        {
            await _staffRepository.UpdateAsync(staff);
        }




        public async Task DeleteStaffAsync(int businessId, int staffId)
        {
            await _staffRepository.DeleteAsync(businessId, staffId);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            var staff = await _staffRepository.GetByEmailAsync(email);
            if (string.IsNullOrEmpty(staff.Password) || !_hashUtility.VerifyPassword(staff.Password, password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return JwtUtility.GenerateJwtToken(staff, _configuration);
        }

        
    }
}