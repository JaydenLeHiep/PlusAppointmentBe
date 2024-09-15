using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Services.Interfaces.ServicesService;

using PlusAppointment.Models.Enums;
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

            var serviceCategory = await _serviceCategoryRepo.GetServiceCategoryByIdAsync(serviceDto.CategoryId.Value);
            if (serviceCategory == null)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Duration = serviceDto.Duration.Value,
                Price = serviceDto.Price.Value,
                BusinessId = businessId,
                CategoryId = serviceCategory.CategoryId // Set the category ID
            };

            await _servicesRepository.AddServiceAsync(service, businessId);
        }

        public async Task AddListServicesAsync(List<ServiceDto>? servicesDtos, int businessId, string userId, string userRole)
        {
            if (string.IsNullOrEmpty(userId) || userRole != Role.Owner.ToString())
            {
                throw new UnauthorizedAccessException("User not authorized");
            }

            if (servicesDtos == null || !servicesDtos.Any())
            {
                throw new ArgumentException("No data provided.");
            }

            var services = new List<Service>();

            foreach (var serviceDto in servicesDtos)
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

                var serviceCategory = await _serviceCategoryRepo.GetServiceCategoryByIdAsync(serviceDto.CategoryId.Value);
                if (serviceCategory == null)
                {
                    throw new ArgumentException("Invalid category ID.");
                }

                var service = new Service
                {
                    Name = serviceDto.Name,
                    Description = serviceDto.Description,
                    Duration = serviceDto.Duration.Value,
                    Price = serviceDto.Price.Value,
                    BusinessId = businessId,
                    CategoryId = serviceCategory.CategoryId // Set the category ID
                };

                services.Add(service);
            }

            await _servicesRepository.AddListServicesAsync(services, businessId);
        }


        public async Task UpdateServiceAsync(int businessId, int serviceId, ServiceDto? serviceDto, string userId)
        {
            if (serviceDto == null)
            {
                throw new ArgumentException("No data provided.");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authorized");
            }

            var service = await _servicesRepository.GetByBusinessIdServiceIdAsync(businessId, serviceId);
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

            if (serviceDto.CategoryId.HasValue)
            {
                var serviceCategory = await _serviceCategoryRepo.GetServiceCategoryByIdAsync(serviceDto.CategoryId.Value);
                if (serviceCategory == null)
                {
                    throw new ArgumentException("Invalid category ID.");
                }
                service.CategoryId = serviceDto.CategoryId.Value;
            }

            await _servicesRepository.UpdateAsync(service);
        }

        public async Task DeleteServiceAsync(int businessId, int serviceId)
        {
            await _servicesRepository.DeleteAsync(businessId, serviceId);
        }
    }
}
