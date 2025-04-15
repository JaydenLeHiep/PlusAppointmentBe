using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Services;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Services.Interfaces.ServicesService;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;

namespace PlusAppointment.Services.Implementations.ServicesService
{
    public class ServicesService : IServicesService
    {
        private readonly IServicesRepository _servicesRepository;
        private readonly IServiceCategoryRepo _serviceCategoryRepo;

        public ServicesService(IServicesRepository servicesRepository, IServiceCategoryRepo serviceCategoryRepo)
        {
            _servicesRepository = servicesRepository;
            _serviceCategoryRepo = serviceCategoryRepo;
        }

        public async Task<IEnumerable<ServiceDto?>> GetAllServicesAsync()
        {
            var services = await _servicesRepository.GetAllAsync();
            return services.Select(s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Description = s.Description,
                Duration = s.Duration,
                Price = s.Price,
                CategoryId = s.CategoryId,
                CategoryName = s.Category?.Name // Include CategoryName in the DTO
            }).ToList();
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(int id)
        {
            var service = await _servicesRepository.GetByIdAsync(id);
            if (service == null)
            {
                return null;
            }

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Duration = service.Duration,
                Price = service.Price,
                CategoryId = service.CategoryId,
                CategoryName = service.Category?.Name // Include CategoryName in the DTO
            };
        }

        public async Task<IEnumerable<ServiceDto?>> GetAllServiceByBusinessIdAsync(int businessId)
        {
            var services = await _servicesRepository.GetAllByBusinessIdAsync(businessId);
            return services.Select(s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Description = s.Description,
                Duration = s.Duration,
                Price = s.Price,
                CategoryId = s.CategoryId,
                CategoryName = s.Category?.Name // Include CategoryName in the DTO
            }).ToList();
        }

        public async Task AddServiceAsync(Service? service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service cannot be null.");
            }

            if (service.CategoryId != null)
            {
                var serviceCategory = await _serviceCategoryRepo.GetServiceCategoryByIdAsync(service.CategoryId.Value);
                if (serviceCategory == null)
                {
                    throw new KeyNotFoundException("Invalid category ID.");
                }
            }

            await _servicesRepository.AddServiceAsync(service);
        }


        public async Task AddListServicesAsync(List<Service>? services)
        {
            if (services == null || !services.Any())
            {
                throw new ArgumentException("No data provided.");
            }

            foreach (var service in services.Where(service => service.CategoryId != null))
            {
                if (service.CategoryId != null)
                {
                    var serviceCategory = await _serviceCategoryRepo.GetServiceCategoryByIdAsync(service.CategoryId.Value);
                    if (serviceCategory == null)
                    {
                        throw new KeyNotFoundException("Invalid category ID.");
                    }
                }
            }

            await _servicesRepository.AddListServicesAsync(services);
        }
        
        public async Task UpdateServiceAsync(Service? service)
        {
            if (service?.CategoryId != null)
            {
                var serviceCategory = await _serviceCategoryRepo.GetServiceCategoryByIdAsync(service.CategoryId.Value);
                if (serviceCategory == null)
                {
                    throw new KeyNotFoundException("Invalid category ID.");
                }
            }

            if (service != null) await _servicesRepository.UpdateAsync(service);
        }
        public async Task<Service?> GetByBusinessIdServiceIdAsync(int businessId, int serviceId)
        {
            return await _servicesRepository.GetByBusinessIdServiceIdAsync(businessId, serviceId);
        }

        public async Task DeleteServiceAsync(int businessId, int serviceId)
        {
            await _servicesRepository.DeleteAsync(businessId, serviceId);
        }
    }
}
