using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using WebApplication1.Services.Interfaces.ServicesService;

namespace WebApplication1.Controllers.ServiceController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IServicesService _servicesService;

        public ServiceController(IServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var services = await _servicesService.GetAllServicesAsync();
            return Ok(services);
        }

        [HttpGet("service_id={id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _servicesService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            return Ok(service);
        }

        [HttpPost("business_id={id}/add")]
        [Authorize]
        public async Task<IActionResult> AddService([FromRoute] int businessId, [FromBody] ServiceDto? serviceDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
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

        [HttpPost("business_id={id}/addList")]
        [Authorize]
        public async Task<IActionResult> AddServices([FromRoute] int businessId, [FromBody] ServicesDto? servicesDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
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

        [HttpPut("business_id={id}")]
        [Authorize]
        public async Task<IActionResult> Update(int businessId, [FromBody] ServiceDto? serviceDto)
        {
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
                await _servicesService.DeleteServiceAsync(businessId, userId);
                return Ok(new { message = "Service deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Delete failed: {ex.Message}" });
            }
        }
    }
}