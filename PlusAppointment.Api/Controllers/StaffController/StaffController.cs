using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.DTOs.Staff;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.StaffService;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.StaffController
{
    [ApiController]
    [Route("api/[controller]")]
     // Ensure all actions require authentication
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public StaffController(IStaffService staffService, IHubContext<AppointmentHub> hubContext)
        {
            _staffService = staffService;
            _hubContext = hubContext;
        }
        
        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString())
            {
                return NotFound(new { message = "You are not authorized to view all staff." });
            }

            var staffs = await _staffService.GetAllStaffsAsync();
            return Ok(staffs);
        }
        
        [Authorize]
        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetById(int staffId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to view this staff." });
            }

            try
            {
                var staff = await _staffService.GetStaffIdAsync(staffId);
                return Ok(staff);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        // Get all staff by business ID
        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetAllStaffByBusinessId(int businessId)
        {
            // var userRole = HttpContext.Items["UserRole"]?.ToString();
            // if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            // {
            //     return NotFound(new { message = "You are not authorized to view this staff." });
            // }

            var staff = await _staffService.GetAllStaffByBusinessIdAsync(businessId);

            return Ok(staff);
        }
        
        [Authorize] 
        [HttpPost("business/{businessId}")]
        public async Task<IActionResult> AddStaff(int businessId, [FromBody] StaffDto? staffDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to add staff." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            
            try
            {
                await _staffService.AddStaffAsync(staffDto, businessId);
                await _hubContext.Clients.All.SendAsync("ReceiveStaffUpdate", "A new staff has been added.");
                return Ok(new { message = "Staff added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize] 
        [HttpPost("business/{businessId}/bulk")]
        public async Task<IActionResult> AddStaffs(int businessId, [FromBody] IEnumerable<StaffDto?> staffDtos)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to add staff." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _staffService.AddListStaffsAsync(staffDtos, businessId);
                await _hubContext.Clients.All.SendAsync("ReceiveStaffUpdate", "A new staff has been added.");
                return Ok(new { message = "Staffs added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize] 
        [HttpPut("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> Update(int businessId, int staffId, [FromBody] StaffDto staffDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to update this staff." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _staffService.UpdateStaffAsync(businessId, staffId, staffDto);
                return Ok(new { message = "Staff updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize] 
        [HttpDelete("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> Delete(int businessId, int staffId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to delete this staff." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _staffService.DeleteStaffAsync(businessId, staffId);
                return Ok(new { message = "Staff deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize] 
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StaffLoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                {
                    return BadRequest(new { message = "Email and Password cannot be null or empty." });
                }
                var token = await _staffService.LoginAsync(loginDto.Email, loginDto.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
