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

        public async Task AddStaffAsync(StaffDto? staffDto, int businessId)
        {
            ValidateStaffDto(staffDto);


            if (staffDto != null)
            {
                var staff = CreateStaffFromDto(staffDto, businessId);

                await _staffRepository.AddStaffAsync(staff, businessId);
            }
        }

        public async Task AddListStaffsAsync(IEnumerable<StaffDto?> staffDtos, int businessId)
        {
            if (staffDtos == null)
            {
                throw new ArgumentNullException(nameof(staffDtos), "Staff list cannot be null");
            }

            var staffDtoList = staffDtos.ToList();

            foreach (var staffDto in staffDtoList)
            {
                ValidateStaffDto(staffDto);


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
                staff.Password = _hashUtility.HashPassword(staffDto.Password);
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
            if (string.IsNullOrEmpty(staff.Password) || !_hashUtility.VerifyPassword(staff.Password, password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return JwtUtility.GenerateJwtToken(staff, _configuration);
        }

        private void ValidateStaffDto(StaffDto? staffDto)
        {
            if (staffDto == null)
            {
                throw new ArgumentNullException(nameof(staffDto), "Staff DTO cannot be null");
            }
            

            if (string.IsNullOrEmpty(staffDto.Password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(staffDto.Password));
            }
        }

        private Staff CreateStaffFromDto(StaffDto? staffDto, int businessId)
        {

            if (string.IsNullOrEmpty(staffDto?.Password))
            {
                throw new ArgumentNullException(nameof(staffDto.Password), "Password cannot be null or empty.");
            }
            
            return new Staff
            {
                Name = staffDto.Name ?? throw new ArgumentNullException(nameof(staffDto.Name), "Name cannot be null."),
                Email = staffDto.Email,
                Phone = staffDto.Phone,
                Password = _hashUtility.HashPassword(staffDto.Password),
                BusinessId = businessId
            };
        }
    }
}