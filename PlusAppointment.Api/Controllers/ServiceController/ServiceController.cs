using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Services.Interfaces.ServicesService;

namespace WebApplication1.Controllers.ServiceController
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Ensure all actions require authentication
    public class ServiceController : ControllerBase
    {
        private readonly IServicesService _servicesService;

        public ServiceController(IServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString())
            {
                return NotFound(new { message = "You are not authorized to view all services." });
            }

            var services = await _servicesService.GetAllServicesAsync();
            return Ok(services);
        }

        [HttpGet("service_id={serviceId}")]
        public async Task<IActionResult> GetById(int serviceId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to view this service." });
            }

            var service = await _servicesService.GetServiceByIdAsync(serviceId);
            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            return Ok(service);
        }
        
        // Get all service by business ID
        [HttpGet("business_id={businessId}")]
        public async Task<IActionResult> GetAllServiceByBusinessId(int businessId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to view this staff." });
            }

            var services = await _servicesService.GetAllServiceByBusinessIdAsync(businessId);
            if (!services.Any())
            {
                return NotFound(new { message = "No staff found for this business." });
            }
            return Ok(services);
        }
        
        [HttpPost("business_id={businessId}/add")]
        public async Task<IActionResult> AddService(int businessId, [FromBody] ServiceDto? serviceDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to add a service." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _servicesService.AddServiceAsync(serviceDto, businessId, userId, userRole);
                return Ok(new { message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("business_id={businessId}/addList")]
        public async Task<IActionResult> AddServices(int businessId, [FromBody] ServicesDto? servicesDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to add services." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _servicesService.AddListServicesAsync(servicesDto, businessId, userId, userRole);
                return Ok(new { message = "Services created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("business_id={businessId}")]
        public async Task<IActionResult> Update(int businessId, [FromBody] ServiceDto? serviceDto)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to update this service." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _servicesService.UpdateServiceAsync(businessId, serviceDto, userId);
                return Ok(new { message = "Service updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [HttpDelete("business_id={businessId}/service_id={serviceId}")]
        public async Task<IActionResult> DeleteService(int businessId, int serviceId)
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            {
                return NotFound(new { message = "You are not authorized to delete this service." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _servicesService.DeleteServiceAsync(businessId, serviceId);
                return Ok(new { message = "Service deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Delete failed: {ex.Message}" });
            }
        }
    }
}
