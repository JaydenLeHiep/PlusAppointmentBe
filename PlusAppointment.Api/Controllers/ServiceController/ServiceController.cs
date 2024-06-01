using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Models;
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

        [HttpGet("{id}")]
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

        [HttpPost("{id}/add")]
        [Authorize]
        public async Task<IActionResult> AddService([FromRoute] int id, [FromBody] ServiceDto? serviceDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                return Unauthorized(new { message = "User not authorized" });
            }
    
            if (serviceDto == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            if (string.IsNullOrEmpty(serviceDto.Name))
            {
                return BadRequest(new { message = "Service name is required." });
            }

            if (!serviceDto.Duration.HasValue)
            {
                return BadRequest(new { message = "Service duration is required." });
            }

            if (!serviceDto.Price.HasValue)
            {
                return BadRequest(new { message = "Service price is required." });
            }

            var businessId = id;
            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Duration = serviceDto.Duration.Value,
                Price = serviceDto.Price.Value,
                BusinessId = businessId
            };

            try
            {
                await _servicesService.AddServiceAsync(service, businessId);
                return Ok(new { message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/addList")]
        [Authorize]
        public async Task<IActionResult> AddServices([FromRoute] int id, [FromBody] ServicesDto? servicesDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            if (servicesDto == null || !servicesDto.Services.Any())
            {
                return BadRequest(new { message = "No data provided." });
            }

            var businessId = id;
            var services = new List<Service>();

            foreach (var serviceDto in servicesDto.Services)
            {
                if (string.IsNullOrEmpty(serviceDto.Name))
                {
                    return BadRequest(new { message = "Service name is required." });
                }

                if (!serviceDto.Duration.HasValue)
                {
                    return BadRequest(new { message = "Service duration is required." });
                }

                if (!serviceDto.Price.HasValue)
                {
                    return BadRequest(new { message = "Service price is required." });
                }

                var service = new Service
                {
                    Name = serviceDto.Name,
                    Description = serviceDto.Description,
                    Duration = serviceDto.Duration.Value,
                    Price = serviceDto.Price.Value,
                    BusinessId = businessId
                };

                services.Add(service);
            }

            try
            {
                await _servicesService.AddListServicesAsync(services, businessId);
                return Ok(new { message = "Services created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceDto? serviceDto)
        {
            if (serviceDto == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var service = await _servicesService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            if (!string.IsNullOrEmpty(serviceDto.Name))
            {
                service.Name = serviceDto.Name;
            }

            if (!string.IsNullOrEmpty(serviceDto.Description))
            {
                service.Description = serviceDto.Description;
            }

            if (serviceDto.Duration.HasValue)
            {
                service.Duration = serviceDto.Duration.Value;
            }

            if (serviceDto.Price.HasValue)
            {
                service.Price = serviceDto.Price.Value;
            }

            try
            {
                await _servicesService.UpdateServiceAsync(service);
                return Ok(new { message = "Service updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
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

            var service = await _servicesService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }
            await _servicesService.DeleteServiceAsync(id);
            return Ok(new { message = "Service deleted successfully" });
        }
    }
}
