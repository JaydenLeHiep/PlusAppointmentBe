using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PlusAppointment.Models.DTOs;
using WebApplication1.Models;

using WebApplication1.Services.Interfaces.StaffService;

namespace WebApplication1.Controllers
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

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var staff = await _staffService.GetStaffIdAsync(id);
            if (staff == null)
            {
                return NotFound(new { message = "Staff not found" });
            }

            return Ok(staff);
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddStaff([FromBody] StaffDto staffDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var staff = new Staff
            {
                Name = staffDto.Name,
                Email = staffDto.Email,
                Phone = staffDto.Phone
            };

            try
            {
                await _staffService.AddStaffAsync(staff, staffDto.BusinessId);
                return Ok(new { message = "Staff added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("addList")]
        [Authorize]
        public async Task<IActionResult> AddStaffs([FromBody] IEnumerable<StaffDto> staffDtos)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var staffs = staffDtos.Select(staffDto => new Staff
            {
                Name = staffDto.Name,
                Email = staffDto.Email,
                Phone = staffDto.Phone
            }).ToList();

            var businessId = staffDtos.FirstOrDefault()?.BusinessId ?? 0;

            try
            {
                await _staffService.AddListStaffsAsync(staffs, businessId);
                return Ok(new { message = "Staffs added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] StaffDto staffDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var staff = await _staffService.GetStaffIdAsync(id);
            if (staff == null)
            {
                return NotFound(new { message = "Staff not found" });
            }

            staff.Name = staffDto.Name;
            staff.Email = staffDto.Email;
            staff.Phone = staffDto.Phone;
            await _staffService.UpdateStaffAsync(staff);
            return Ok(new { message = "Staff updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var staff = await _staffService.GetStaffIdAsync(id);
            await _staffService.DeleteStaffAsync(id);
            return Ok(new { message = "Staff deleted successfully" });
        }
    }
}
