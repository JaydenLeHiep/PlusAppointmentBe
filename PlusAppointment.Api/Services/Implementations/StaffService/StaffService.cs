using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
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
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                throw new KeyNotFoundException("Staff not found");
            }

            return staff;
        }

        public async Task<IEnumerable<Staff?>> GetAllStaffByBusinessIdAsync(int businessId)
        {
            return await _staffRepository.GetAllByBusinessIdAsync(businessId);
        }
        
        public async Task<Staff?> GetStaffByBusinessAndStaffIdAsync(int staffId, int businessId)
        {
            return await _staffRepository.GetStaffByBusinessAndStaffIdAsync(staffId, businessId);
        }

        public async Task AddStaffAsync(StaffDto staffDto)
        {
            ValidateStaffDto(staffDto);

            // if (staffDto.Email != null && await _staffRepository.EmailExistsAsync(staffDto.Email))
            // {
            //     throw new Exception("Email already exists");
            // }
            //
            // if (staffDto.Phone != null && await _staffRepository.PhoneExistsAsync(staffDto.Phone))
            // {
            //     throw new Exception("Phone number already exists");
            // }

            var staff = CreateStaffFromDto(staffDto, staffDto.BusinessId);

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
                ValidateStaffDto(staffDto);

                if (staffDto.Email != null && await _staffRepository.EmailExistsAsync(staffDto.Email))
                {
                    throw new Exception($"Email {staffDto.Email} already exists");
                }

                if (staffDto.Phone != null && await _staffRepository.PhoneExistsAsync(staffDto.Phone))
                {
                    throw new Exception($"Phone number {staffDto.Phone} already exists");
                }
            }

            var staffs = staffDtoList.Select(staffDto => CreateStaffFromDto(staffDto, businessId)).ToList();

            await _staffRepository.AddListStaffsAsync(staffs);
        }

        public async Task UpdateStaffAsync(int businessId, int staffId, StaffDto staffDto)
        {
            if (staffDto == null)
            {
                throw new ArgumentException("No data provided.");
            }

            var staff = await _staffRepository.GetByBusinessIdServiceIdAsync(businessId, staffId);
            if (staff == null)
            {
                throw new KeyNotFoundException("Staff not found");
            }

            // Update only if new values are provided, or if the value is explicitly cleared
            if (staffDto.Name != null) // Update if the user provides a new name
            {
                staff.Name = staffDto.Name;
            }

            if (staffDto.Email != null) // Update or clear the email
            {
                staff.Email = staffDto.Email; // Set to the new email or empty string if user clears it
            }

            if (staffDto.Phone != null) // Update or clear the phone
            {
                staff.Phone = staffDto.Phone; // Set to the new phone or empty string if user clears it
            }

            if (!string.IsNullOrEmpty(staffDto.Password)) // Update the password if provided
            {
                staff.Password = HashUtility.HashPassword(staffDto.Password);
            }

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
            if (string.IsNullOrEmpty(staff.Password) || !HashUtility.VerifyPassword(staff.Password, password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return JwtUtility.GenerateJwtToken(staff, _configuration);
        }

        private void ValidateStaffDto(StaffDto staffDto)
        {
            if (staffDto == null)
            {
                throw new ArgumentNullException(nameof(staffDto), "Staff DTO cannot be null");
            }

            // if (string.IsNullOrEmpty(staffDto.Email))
            // {
            //     throw new ArgumentException("Email cannot be null or empty.", nameof(staffDto.Email));
            // }
            //
            // if (string.IsNullOrEmpty(staffDto.Phone))
            // {
            //     throw new ArgumentException("Phone cannot be null or empty.", nameof(staffDto.Phone));
            // }

            if (string.IsNullOrEmpty(staffDto.Password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(staffDto.Password));
            }
        }

        private Staff CreateStaffFromDto(StaffDto staffDto, int businessId)
        {
            // if (string.IsNullOrEmpty(staffDto.Email))
            // {
            //     throw new ArgumentNullException(nameof(staffDto.Email), "Email cannot be null or empty.");
            // }
            //
            // if (string.IsNullOrEmpty(staffDto.Phone))
            // {
            //     throw new ArgumentNullException(nameof(staffDto.Phone), "Phone cannot be null or empty.");
            // }

            if (string.IsNullOrEmpty(staffDto.Password))
            {
                throw new ArgumentNullException(nameof(staffDto.Password), "Password cannot be null or empty.");
            }

            return new Staff
            {
                Name = staffDto.Name ?? throw new ArgumentNullException(nameof(staffDto.Name), "Name cannot be null."),
                Email = staffDto.Email,
                Phone = staffDto.Phone,
                Password = HashUtility.HashPassword(staffDto.Password),
                BusinessId = businessId
            };
        }
    }
}