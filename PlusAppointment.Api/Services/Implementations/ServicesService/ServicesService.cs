using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Repositories.Interfaces.ServicesRepo;
using WebApplication1.Services.Interfaces.ServicesService;

namespace WebApplication1.Services.Implementations.ServicesService
{
    public class ServicesService : IServicesService
    {
        private readonly IServicesRepository _servicesRepository;

        public ServicesService(IServicesRepository servicesRepository)
        {
            _servicesRepository = servicesRepository;
        }

        public async Task<IEnumerable<Service?>> GetAllServicesAsync()
        {
            return await _servicesRepository.GetAllAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _servicesRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Service?>> GetAllServiceByBusinessIdAsync(int businessId)
        {
            return await _servicesRepository.GetAllByBusinessIdAsync(businessId);
        }

        public async Task AddServiceAsync(ServiceDto? serviceDto, int businessId, string userId, string userRole)
        {
            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                throw new UnauthorizedAccessException("User not authorized");
            }

            if (serviceDto == null)
            {
                throw new ArgumentException("No data provided.");
            }

            if (string.IsNullOrEmpty(serviceDto.Name))
            {
                throw new ArgumentException("Service name is required.");
            }

            if (!serviceDto.Duration.HasValue)
            {
                throw new ArgumentException("Service duration is required.");
            }

            if (!serviceDto.Price.HasValue)
            {
                throw new ArgumentException("Service price is required.");
            }

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Duration = serviceDto.Duration.Value,
                Price = serviceDto.Price.Value,
                BusinessId = businessId
            };

            await _servicesRepository.AddServiceAsync(service, businessId);
        }

        public async Task AddListServicesAsync(ServicesDto? servicesDto, int businessId, string userId, string userRole)
        {
            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                throw new UnauthorizedAccessException("User not authorized");
            }

            if (servicesDto == null || !servicesDto.Services.Any())
            {
                throw new ArgumentException("No data provided.");
            }

            var services = new List<Service>();

            foreach (var serviceDto in servicesDto.Services)
            {
                if (string.IsNullOrEmpty(serviceDto.Name))
                {
                    throw new ArgumentException("Service name is required.");
                }

                if (!serviceDto.Duration.HasValue)
                {
                    throw new ArgumentException("Service duration is required.");
                }

                if (!serviceDto.Price.HasValue)
                {
                    throw new ArgumentException("Service price is required.");
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

            await _servicesRepository.AddListServicesAsync(services, businessId);
        }

        public async Task UpdateServiceAsync(int id, ServiceDto? serviceDto, string userId)
        {
            if (serviceDto == null)
            {
                throw new ArgumentException("No data provided.");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authorized");
            }

            var service = await _servicesRepository.GetByIdAsync(id);
            if (service == null)
            {
                throw new Exception("Service not found");
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

            await _servicesRepository.UpdateAsync(service);
        }

        public async Task DeleteServiceAsync(int businessId, int serviceId)
        {
            await _servicesRepository.DeleteAsync(businessId, serviceId);
        }


    }
}