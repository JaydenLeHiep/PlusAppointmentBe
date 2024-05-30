using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Models;

using WebApplication1.Services.Interfaces.ServicesService;

namespace WebApplication1.Controllers
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

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddService([FromBody] ServiceDto serviceDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Duration = serviceDto.Duration,
                Price = serviceDto.Price
            };

            try
            {
                await _servicesService.AddServiceAsync(service, serviceDto.BusinessId);
                return Ok(new { message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("addList")]
        [Authorize]
        public async Task<IActionResult> AddServices([FromBody] ServicesDto servicesDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var services = servicesDto.Services.Select(serviceDto => new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Duration = serviceDto.Duration,
                Price = serviceDto.Price
            }).ToList();

            var businessId = servicesDto.Services.FirstOrDefault()?.BusinessId ?? 0;

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
        public async Task<IActionResult> Update(int id, [FromBody] ServiceDto serviceDto)
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

            service.Name = serviceDto.Name;
            service.Description = serviceDto.Description;
            service.Duration = serviceDto.Duration;
            service.Price = serviceDto.Price;
            await _servicesService.UpdateServiceAsync(service);
            return Ok(new { message = "Service updated successfully" });
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
            await _servicesService.DeleteServiceAsync(id);
            return Ok(new { message = "Service deleted successfully" });
        }
    }
}
