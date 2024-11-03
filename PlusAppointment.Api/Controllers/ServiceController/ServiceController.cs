using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.ServicesService;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.ServiceController
{
    [ApiController]
    [Route("api/[controller]")]
     // Ensure all actions require authentication
    public class ServiceController : ControllerBase
    {
        private readonly IServicesService _servicesService;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public ServiceController(IServicesService servicesService, IHubContext<AppointmentHub> hubContext)
        {
            _servicesService = servicesService;
            _hubContext = hubContext;
        }
        [Authorize] 
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
        [Authorize] 
        [HttpGet("{serviceId}")]
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
        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetAllServiceByBusinessId(int businessId)
        {
            // var userRole = HttpContext.Items["UserRole"]?.ToString();
            // if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
            // {
            //     return NotFound(new { message = "You are not authorized to view this service." });
            // }

            var services = await _servicesService.GetAllServiceByBusinessIdAsync(businessId);

            return Ok(services);
        }
        [Authorize] 
        [HttpPost("business/{businessId}")]
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
                await _hubContext.Clients.All.SendAsync("ReceiveServiceUpdate", "A new service has been added.");
                return Ok(new { message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpPost("business/{businessId}/addList")]
        public async Task<IActionResult> AddServices(int businessId, [FromBody] List<ServiceDto>? servicesDtos)
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
                await _servicesService.AddListServicesAsync(servicesDtos, businessId, userId, userRole);
                return Ok(new { message = "Services created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("business/{businessId}/service/{serviceId}")]
        public async Task<IActionResult> UpdateService(int businessId, int serviceId, [FromBody] ServiceDto? serviceDto)
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
                await _servicesService.UpdateServiceAsync(businessId, serviceId, serviceDto, userId);
                return Ok(new { message = "Service updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [Authorize] 
        [HttpDelete("business/{businessId}/service/{serviceId}")]
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
