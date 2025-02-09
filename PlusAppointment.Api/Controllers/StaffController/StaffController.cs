
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Staff;
using PlusAppointment.Services.Interfaces.StaffService;
using PlusAppointment.Utils.Hash;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.StaffController
{
    [ApiController]
    [Route("api/staffs")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly IHubContext<AppointmentHub> _hubContext;
        private readonly IHashUtility _hashUtility;

        public StaffController(IStaffService staffService, IHubContext<AppointmentHub> hubContext, IHashUtility hashUtility)
        {
            _staffService = staffService;
            _hubContext = hubContext;
            _hashUtility = hashUtility;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var staffs = await _staffService.GetAllStaffsAsync();
            if (!staffs.Any())
            {
                return NotFound(new { message = "No staff found." });
            }
            return Ok(staffs);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetById(int staffId)
        {
            var staff = await _staffService.GetStaffIdAsync(staffId);
            return Ok(staff);
        }

        [HttpGet("businesses/{businessId}")]
        public async Task<IActionResult> GetAllStaffByBusinessId(int businessId)
        {
            var staff = await _staffService.GetAllStaffByBusinessIdAsync(businessId);
            return Ok(staff);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost("businesses/{businessId}")]
        public async Task<IActionResult> AddStaff(int businessId, [FromBody] StaffDto? staffDto)
        {
            if (staffDto == null)
            {
                return BadRequest(new { error = "Validation Error", message = "No data provided." });
            }

            // Convert DTO to entity inside the controller
            var staff = new Staff
            {
                Name = staffDto.Name,
                Email = staffDto.Email,
                Phone = staffDto.Phone,
                BusinessId = businessId
            };

            await _staffService.AddStaffAsync(staff, businessId);
            await _hubContext.Clients.All.SendAsync("ReceiveStaffUpdate", "A new staff has been added.");

            return CreatedAtAction(nameof(GetById), new { staffId = staff.StaffId }, 
                new { message = "Staff added successfully.", staffId = staff.StaffId });
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost("businesses/{businessId}/bulk")]
        public async Task<IActionResult> AddStaffs(int businessId, [FromBody] IEnumerable<StaffDto>? staffDtos)
        {
            if (staffDtos != null)
            {
                var enumerable = staffDtos.ToList();
                if (!enumerable.Any())
                {
                    return BadRequest(new { error = "Validation Error", message = "Staff list cannot be empty." });
                }

                var staffList = enumerable
                    .Select(staffDto => new Staff
                    {
                        Name = staffDto.Name ?? throw new ArgumentException("Name is required."),
                        Email = staffDto.Email,
                        Phone = staffDto.Phone,
                    
                        BusinessId = businessId
                    }).ToList();

                await _staffService.AddListStaffsAsync(staffList);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveStaffUpdate", "New staff members have been added.");

            return Created("", new { message = "Staffs added successfully." });
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("businesses/{businessId}/staffs/{staffId}")]
        public async Task<IActionResult> Update(int businessId, int staffId, [FromBody] StaffDto? staffDto)
        {
            if (staffDto == null)
            {
                return BadRequest(new { error = "Validation Error", message = "No data provided." });
            }

            // Fetch the existing staff member
            var staff = await _staffService.GetByBusinessIdStaffIdAsync(businessId, staffId);
            if (staff == null)
            {
                return NotFound(new { error = "Not Found", message = "Staff not found." });
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(staffDto.Name))
            {
                staff.Name = staffDto.Name;
            }

            if (!string.IsNullOrEmpty(staffDto.Email))
            {
                staff.Email = staffDto.Email;
            }

            if (!string.IsNullOrEmpty(staffDto.Phone))
            {
                staff.Phone = staffDto.Phone;
            }

            if (!string.IsNullOrEmpty(staffDto.Password))
            {
                staff.Password = _hashUtility.HashPassword(staffDto.Password);
            }

            await _staffService.UpdateStaffAsync(staff);
            return Ok(new { message = "Staff updated successfully." });
        }


        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("businesses/{businessId}/staffs/{staffId}")]
        public async Task<IActionResult> Delete(int businessId, int staffId)
        {
            await _staffService.DeleteStaffAsync(businessId, staffId);
            return Ok(new { message = "Staff deleted successfully." });
        }

        [Authorize]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StaffLoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest(new { message = "Email and Password cannot be null or empty." });
            }

            var token = await _staffService.LoginAsync(loginDto.Email, loginDto.Password);
            return Ok(new { token });
        }
        
    }
}
