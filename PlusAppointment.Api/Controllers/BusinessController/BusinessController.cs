using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.BusinessService;


namespace PlusAppointment.Controllers.BusinessController
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllAdmin()
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString())
            {
                return NotFound(new { message = "You are not authorized to view all businesses." });
            }

            var businesses = await _businessService.GetAllBusinessesAsync();
            if (!businesses.Any())
            {
                return NotFound(new { message = "No businesses found." });
            }
            return Ok(businesses);
        }

        [Authorize]
        [HttpGet("byUser")]
        public async Task<IActionResult> GetAllByUser()
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to view this business." });
            }
            var businesses = await _businessService.GetAllBusinessesByUserIdAsync(currentUserId);
            if (!businesses.Any())
            {
                return NotFound(new { message = "No businesses found for this user." });
            }
            return Ok(businesses);
        }

        [HttpGet("business_id={businessId}")]
        public async Task<IActionResult> GetById(int businessId)
        {
            var business = await _businessService.GetBusinessByIdAsync(businessId);
            if (business == null)
            {
                return NotFound(new { message = "Business not found." });
            }
            return Ok(business);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BusinessDto? businessDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to create a business." });
            }

            if (businessDto == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized." });
            }

            await _businessService.AddBusinessAsync(businessDto, int.Parse(userId));
            return Ok(new { message = "Business created successfully." });
        }

        [Authorize]
        [HttpPut("business_id={businessId}")]
        public async Task<IActionResult> Update(int businessId, [FromBody] BusinessDto? businessDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to update this business." });
            }

            if (businessDto == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            try
            {
                await _businessService.UpdateBusinessAsync(businessId, businessDto, currentUserId, userRole);
                return Ok(new { message = "Business updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpDelete("business_id={businessId}")]
        public async Task<IActionResult> Delete(int businessId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to delete this business." });
            }

            try
            {
                await _businessService.DeleteBusinessAsync(businessId, currentUserId, userRole);
                return Ok(new { message = "Business deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Delete failed: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpGet("business_id={businessId}/services")]
        public async Task<IActionResult> GetServices(int businessId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to view the services of this business." });
            }

            var services = await _businessService.GetServicesByBusinessIdAsync(businessId);
            if (!services.Any())
            {
                return NotFound(new { message = "No services found for this business." });
            }

            return Ok(services);
        }

        [Authorize]
        [HttpGet("business_id={businessId}/staff")]
        public async Task<IActionResult> GetStaff(int businessId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to view the staff of this business." });
            }

            var staff = await _businessService.GetStaffByBusinessIdAsync(businessId);
            if (!staff.Any())
            {
                return NotFound(new { message = "No staff found for this business." });
            }

            return Ok(staff);
        }
    }
}
