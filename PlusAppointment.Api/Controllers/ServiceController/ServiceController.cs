using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Services;
using PlusAppointment.Services.Interfaces.ServicesService;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.ServiceController
{
    [ApiController]
    [Route("api/services")]
    public class ServiceController : ControllerBase
    {
        private readonly IServicesService _servicesService;
        private readonly IHubContext<AppointmentHub> _hubContext;
        

        public ServiceController(IServicesService servicesService, IHubContext<AppointmentHub> hubContext)
        {
            _servicesService = servicesService;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _servicesService.GetAllServicesAsync();
            if (!services.Any())
            {
                return NotFound(new { message = "No services found." });
            }
            return Ok(services);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetById(int serviceId)
        {
            var service = await _servicesService.GetServiceByIdAsync(serviceId);
            return Ok(service);
        }

        [HttpGet("businesses/{businessId}")]
        public async Task<IActionResult> GetAllServiceByBusinessId(int businessId)
        {
            var services = await _servicesService.GetAllServiceByBusinessIdAsync(businessId);
            return Ok(services);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost("businesses/{businessId}")]
        public async Task<IActionResult> AddService(int businessId, [FromBody] ServiceDto? serviceDto)
        {
            if (serviceDto == null)
            {
                return BadRequest(new { error = "Validation Error", message = "No data provided." });
            }

            if (string.IsNullOrEmpty(serviceDto.Name))
            {
                return BadRequest(new { error = "Validation Error", message = "Service name is required." });
            }

            if (!serviceDto.Duration.HasValue)
            {
                return BadRequest(new { error = "Validation Error", message = "Service duration is required." });
            }

            if (!serviceDto.Price.HasValue)
            {
                return BadRequest(new { error = "Validation Error", message = "Service price is required." });
            }

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Duration = serviceDto.Duration.Value,
                Price = serviceDto.Price.Value,
                BusinessId = businessId,
                CategoryId = serviceDto.CategoryId ?? throw new ArgumentException("Category ID is required.")
            };

            await _servicesService.AddServiceAsync(service);
            await _hubContext.Clients.All.SendAsync("ReceiveServiceUpdate", "A new service has been added.");

            return Created("", new { message = "Service created successfully." });
        }


        [Authorize(Roles = "Admin,Owner")]
        [HttpPost("businesses/{businessId}/bulk")]
        public async Task<IActionResult> AddServices(int businessId, [FromBody] List<ServiceDto>? serviceDtos)
        {
            if (serviceDtos == null || !serviceDtos.Any())
            {
                return BadRequest(new { error = "Validation Error", message = "Service list cannot be empty." });
            }

            var services = new List<Service>();

            foreach (var serviceDto in serviceDtos)
            {
                if (string.IsNullOrEmpty(serviceDto.Name))
                {
                    return BadRequest(new { error = "Validation Error", message = "Service name is required." });
                }

                if (!serviceDto.Duration.HasValue)
                {
                    return BadRequest(new { error = "Validation Error", message = "Service duration is required." });
                }

                if (!serviceDto.Price.HasValue)
                {
                    return BadRequest(new { error = "Validation Error", message = "Service price is required." });
                }

                if (!serviceDto.CategoryId.HasValue)
                {
                    return BadRequest(new { error = "Validation Error", message = "Category ID is required." });
                }

                services.Add(new Service
                {
                    Name = serviceDto.Name,
                    Description = serviceDto.Description,
                    Duration = serviceDto.Duration.Value,
                    Price = serviceDto.Price.Value,
                    BusinessId = businessId,
                    CategoryId = serviceDto.CategoryId.Value
                });
            }

            await _servicesService.AddListServicesAsync(services);
            await _hubContext.Clients.All.SendAsync("ReceiveServiceUpdate", "New services have been added.");

            return Created("", new { message = "Services created successfully." });
        }


        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("businesses/{businessId}/services/{serviceId}")]
        public async Task<IActionResult> UpdateService(int businessId, int serviceId, [FromBody] ServiceDto? serviceDto)
        {
            if (serviceDto == null)
            {
                return BadRequest(new { error = "Validation Error", message = "No data provided." });
            }

            // Fetch existing service
            var service = await _servicesService.GetByBusinessIdServiceIdAsync(businessId, serviceId);
            if (service == null)
            {
                return NotFound(new { error = "Not Found", message = "Service not found." });
            }

            // Update only non-null fields
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

            if (serviceDto.CategoryId.HasValue)
            {
                service.CategoryId = serviceDto.CategoryId.Value;
            }

            await _servicesService.UpdateServiceAsync(service);
            return Ok(new { message = "Service updated successfully." });
        }


        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("businesses/{businessId}/services/{serviceId}")]
        public async Task<IActionResult> DeleteService(int businessId, int serviceId)
        {
            await _servicesService.DeleteServiceAsync(businessId, serviceId);
            return Ok(new { message = "Service deleted successfully." });
        }
    }
}
