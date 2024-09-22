using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentRead;

public class AppointmentReadRepository : IAppointmentReadRepository
{
    private readonly ApplicationDbContext _context;
    private readonly RedisHelper _redisHelper;

    public AppointmentReadRepository(ApplicationDbContext context, RedisHelper redisHelper)
    {
        _context = context;
        _redisHelper = redisHelper;
    }

    private DateTime GetStartOfTodayUtc()
    {
        return DateTime.UtcNow.Date;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        const string cacheKey = "all_appointments";
        var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

        if (cachedAppointments != null && cachedAppointments.Any())
        {
            return cachedAppointments.Select(dto => MapFromCacheDto(dto));
        }

        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Service)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Staff)
            .ToListAsync();

        var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
        await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

        return appointments;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
    {
        // string cacheKey = $"appointment_{appointmentId}";
        // var appointmentCacheDto = await _redisHelper.GetCacheAsync<AppointmentCacheDto>(cacheKey);
        // if (appointmentCacheDto != null)
        // {
        //     return MapFromCacheDto(appointmentCacheDto);
        // }

        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Service)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Staff)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        return appointment;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
    {
        string cacheKey = $"appointments_customer_{customerId}";
        var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

        if (cachedAppointments != null && cachedAppointments.Any())
        {
            return cachedAppointments.Select(dto => MapFromCacheDto(dto));
        }

        var startOfTodayUtc = GetStartOfTodayUtc();
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Service)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Staff)
            .Where(a => a.CustomerId == customerId && a.AppointmentTime >= startOfTodayUtc)
            .ToListAsync();

        var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
        await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

        return appointments;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
    {
        string cacheKey = $"appointments_business_{businessId}";
        var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

        if (cachedAppointments != null && cachedAppointments.Any())
        {
            return cachedAppointments.Select(dto => MapFromCacheDto(dto));
        }

        var startOfTodayUtc = GetStartOfTodayUtc();
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Service)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Staff)
            .Where(a => a.BusinessId == businessId && a.AppointmentTime >= startOfTodayUtc)
            .ToListAsync();

        var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
        await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

        return appointments;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
    {
        string cacheKey = $"appointments_staff_{staffId}";
        var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

        if (cachedAppointments != null && cachedAppointments.Any())
        {
            return cachedAppointments.Select(dto => MapFromCacheDto(dto));
        }

        var startOfTodayUtc = GetStartOfTodayUtc();

        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Service)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Staff)
            .Where(a => a.AppointmentServices != null &&
                        a.AppointmentServices.Any(apptService => apptService.StaffId == staffId) &&
                        a.AppointmentTime >= startOfTodayUtc)
            .ToListAsync();

        var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
        await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

        return appointments;
    }

    public async Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId)
    {
        string cacheKey = $"appointments_customer_{customerId}_history";
        var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

        if (cachedAppointments != null && cachedAppointments.Any())
        {
            return cachedAppointments.Select(dto => MapFromCacheDto(dto));
        }

        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Service)
            .Include(a => a.AppointmentServices)!
            .ThenInclude(apptService => apptService.Staff)
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();

        var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
        await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

        return appointments;
    }

    public async Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        var appointments = await _context.AppointmentServiceStaffs
            .Where(ass => ass.StaffId == staffId && ass.Appointment.AppointmentTime.Date == utcDate)
            .Select(ass => new
            {
                ass.Appointment.AppointmentTime,
                ass.Appointment.Duration
            })
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();

        var notAvailableTimeSlots = new List<DateTime>();

        foreach (var appointment in appointments)
        {
            var appointmentStart = appointment.AppointmentTime;
            var appointmentEnd = appointment.AppointmentTime.Add(appointment.Duration);

            var currentTimeSlot = appointmentStart;

            while (currentTimeSlot < appointmentEnd)
            {
                notAvailableTimeSlots.Add(currentTimeSlot);
                currentTimeSlot = currentTimeSlot.AddMinutes(15);
            }
        }

        return notAvailableTimeSlots;
    }

    public async Task<Customer?> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Customers.FindAsync(customerId);
    }

    public async Task<ServiceCategory?> GetServiceCategoryByIdAsync(int categoryId)
    {
        return await _context.ServiceCategories
            .FirstOrDefaultAsync(sc => sc.CategoryId == categoryId);
    }

    private Appointment MapFromCacheDto(AppointmentCacheDto dto)
    {
        var services = dto.ServiceStaffs.Select(ss => new AppointmentServiceStaffMapping
        {
            ServiceId = ss.ServiceId,
            StaffId = ss.StaffId,
            Service = _context.Services.FirstOrDefault(s => s.ServiceId == ss.ServiceId),
            Staff = _context.Staffs.FirstOrDefault(s => s.StaffId == ss.StaffId)
        }).ToList();

        return new Appointment
        {
            AppointmentId = dto.AppointmentId,
            CustomerId = dto.CustomerId,
            Customer = new Customer
            {
                CustomerId = dto.CustomerId,
                Name = dto.CustomerName,
                Phone = dto.CustomerPhone
            },
            BusinessId = dto.BusinessId,
            AppointmentTime = dto.AppointmentTime,
            Duration = dto.Duration,
            Comment = dto.Comment,
            Status = dto.Status,
            AppointmentServices = services
        };
    }

    private AppointmentCacheDto MapToCacheDto(Appointment appointment)
    {
        var serviceStaffs = appointment.AppointmentServices?
            .Select(apptService => new ServiceStaffCacheDto
            {
                ServiceId = apptService.ServiceId,
                ServiceName = apptService.Service?.Name ?? string.Empty,
                StaffId = apptService.StaffId,
                StaffName = apptService.Staff?.Name ?? string.Empty,
                StaffPhone = apptService.Staff?.Phone ?? string.Empty
            }).ToList() ?? new List<ServiceStaffCacheDto>();

        return new AppointmentCacheDto
        {
            AppointmentId = appointment.AppointmentId,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer?.Name ?? string.Empty,
            CustomerPhone = appointment.Customer?.Phone ?? string.Empty,
            BusinessId = appointment.BusinessId,
            AppointmentTime = appointment.AppointmentTime,
            Duration = appointment.Duration,
            Comment = appointment.Comment ?? string.Empty,
            Status = appointment.Status,
            ServiceStaffs = serviceStaffs
        };
    }
}