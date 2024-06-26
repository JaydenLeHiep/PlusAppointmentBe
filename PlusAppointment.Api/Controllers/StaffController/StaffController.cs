using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Services.Interfaces.StaffService;

namespace WebApplication1.Controllers.StaffController
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var staffs = await _staffService.GetAllStaffsAsync();
            return Ok(staffs);
        }

        [HttpGet("staff_id={id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var staff = await _staffService.GetStaffIdAsync(id);

            return Ok(staff);
        }

        [HttpPost("business_id={id}/add")]
        [Authorize]
        public async Task<IActionResult> AddStaff([FromRoute] int businessId,[FromBody] StaffDto staffDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            staffDto.BusinessId = businessId;
            try
            {
                await _staffService.AddStaffAsync(staffDto);
                return Ok(new { message = "Staff added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("business_id={id}/addList")]
        [Authorize]
        public async Task<IActionResult> AddStaffs([FromRoute] int businessId, [FromBody] IEnumerable<StaffDto> staffDtos)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _staffService.AddListStaffsAsync(staffDtos, businessId);
                return Ok(new { message = "Staffs added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("business_id={id}")]
        [Authorize]
        public async Task<IActionResult> Update(int businessId, [FromBody] StaffDto staffDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _staffService.UpdateStaffAsync(businessId, staffDto);
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

        [HttpDelete("business_id={id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int businessId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _staffService.DeleteStaffAsync(businessId);
                return Ok(new { message = "Staff deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
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
