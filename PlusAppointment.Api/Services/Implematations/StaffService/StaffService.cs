using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.StaffRepo;
using WebApplication1.Services.Interfaces.StaffService;
using PlusAppointment.Models.DTOs;
using System.Linq;
using WebApplication1.Utils.Hash;
using WebApplication1.Utils.Jwt;

namespace WebApplication1.Services.Implematations.StaffService
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

        public async Task AddStaffAsync(StaffDto staffDto)
        {
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

        public async Task AddListStaffsAsync(IEnumerable<StaffDto> staffDtos)
        {
            foreach (var staffDto in staffDtos)
            {
                if (await _staffRepository.EmailExistsAsync(staffDto.Email))
                {
                    throw new Exception($"Email {staffDto.Email} already exists");
                }

                if (await _staffRepository.PhoneExistsAsync(staffDto.Phone))
                {
                    throw new Exception($"Phone number {staffDto.Phone} already exists");
                }
            }

            var staffs = staffDtos.Select(staffDto => new Staff
            {
                Name = staffDto.Name,
                Email = staffDto.Email,
                Phone = staffDto.Phone,
                Password = HashUtility.HashPassword(staffDto.Password)
            }).ToList();

            var businessId = staffDtos.FirstOrDefault()?.BusinessId ?? 0;
            await _staffRepository.AddListStaffsAsync(staffs, businessId);
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
            var staff = await _staffRepository.GetByEmailAsync(email);
            if (staff == null || !HashUtility.VerifyPassword(staff.Password, password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return JwtUtility.GenerateJwtToken(staff, _configuration);
        }
    }
}
