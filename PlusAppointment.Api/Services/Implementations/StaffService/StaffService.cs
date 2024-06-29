using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using WebApplication1.Repositories.Interfaces.StaffRepo;
using WebApplication1.Services.Interfaces.StaffService;
using WebApplication1.Utils.Hash;
using WebApplication1.Utils.Jwt;

namespace WebApplication1.Services.Implementations.StaffService
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IConfiguration _configuration;
        public StaffService(IStaffRepository staffRepository, IConfiguration configuration)
        {
            _staffRepository = staffRepository;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Staff>> GetAllStaffsAsync()
        {
            return await _staffRepository.GetAllAsync();
        }

        public async Task<Staff> GetStaffIdAsync(int id)
        {
            return await _staffRepository.GetByIdAsync(id);
        }
        public async Task<IEnumerable<Staff?>> GetAllStaffByBusinessIdAsync(int businessId)
        {
            return await _staffRepository.GetAllByBusinessIdAsync(businessId);
        }

        public async Task AddStaffAsync(StaffDto staffDto)
        {
            if (string.IsNullOrEmpty(staffDto.Email) || string.IsNullOrEmpty(staffDto.Phone) || string.IsNullOrEmpty(staffDto.Password))
            {
                throw new Exception("Email, Phone, and Password cannot be null or empty.");
            }
            if (await _staffRepository.EmailExistsAsync(staffDto.Email))
            {
                throw new Exception("Email already exists");
            }

            if (await _staffRepository.PhoneExistsAsync(staffDto.Phone))
            {
                throw new Exception("Phone number already exists");
            }

            var staff = new Staff
            {
                Name = staffDto.Name,
                Email = staffDto.Email,
                Phone = staffDto.Phone,
                Password = HashUtility.HashPassword(staffDto.Password)
            };

            await _staffRepository.AddStaffAsync(staff, staffDto.BusinessId);
        }

        public async Task AddListStaffsAsync(IEnumerable<StaffDto> staffDtos, int businessId)
        {
            if (staffDtos == null)
            {
                throw new ArgumentNullException(nameof(staffDtos), "Staff list cannot be null");
            }

            var staffDtoList = staffDtos.ToList();

            foreach (var staffDto in staffDtoList)
            {
                if (string.IsNullOrEmpty(staffDto.Email) || string.IsNullOrEmpty(staffDto.Phone) || string.IsNullOrEmpty(staffDto.Password))
                {
                    throw new Exception("Email, Phone, and Password cannot be null or empty.");
                }

                if (await _staffRepository.EmailExistsAsync(staffDto.Email))
                {
                    throw new Exception($"Email {staffDto.Email} already exists");
                }

                if (await _staffRepository.PhoneExistsAsync(staffDto.Phone))
                {
                    throw new Exception($"Phone number {staffDto.Phone} already exists");
                }
            }

            var staffs = staffDtoList.Select(staffDto =>
            {
                if (staffDto.Password == null)
                {
                    throw new ArgumentNullException(nameof(staffDto.Password), "Password cannot be null.");
                }

                return new Staff
                {
                    Name = staffDto.Name,
                    Email = staffDto.Email,
                    Phone = staffDto.Phone,
                    Password = HashUtility.HashPassword(staffDto.Password),
                    BusinessId = businessId
                };
            }).ToList();

            await _staffRepository.AddListStaffsAsync(staffs);
        }
        public async Task UpdateStaffAsync(int id, StaffDto staffDto)
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                throw new KeyNotFoundException("Staff not found");
            }

            staff.Name = staffDto.Name;
            staff.Email = staffDto.Email;
            staff.Phone = staffDto.Phone;
            await _staffRepository.UpdateAsync(staff);
        }

        public async Task DeleteStaffAsync(int id)
        {
            await _staffRepository.DeleteAsync(id);
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
            if (string.IsNullOrEmpty(staff.Password) || !HashUtility.VerifyPassword(staff.Password, password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return JwtUtility.GenerateJwtToken(staff, _configuration);
        }
    }
}
