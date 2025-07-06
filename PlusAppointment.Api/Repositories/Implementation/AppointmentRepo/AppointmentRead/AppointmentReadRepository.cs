using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentRead;

public class AppointmentReadRepository : IAppointmentReadRepository
{
    private readonly ApplicationDbContext _context;
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public AppointmentReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private DateTime GetStartOfTodayUtc()
    {
        return DateTime.UtcNow.Date;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .ToListAsync();
        
            return appointments;
        }
        catch (Exception e)
        {
            logger.Error("Error while retrieving all appointments: "+ e);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
    {
        try
        {
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
        catch (Exception e)
        {
            logger.Error("Error while retrieving an appointment: "+ e);
            return null;
        }

    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
    {
        try
        {
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

            return appointments;
        }
        catch (Exception e)
        {
            logger.Error("Error while retrieving an appointment by customer id " + customerId + ": " + e);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
    {
        try
        {
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
        
            return appointments;
        }
        catch (Exception e)
        {
            logger.Error("Error while retrieving an appointment by business id " + businessId + ": " + e);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
    {
        try
        {
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

            return appointments;
        }
        catch (Exception e)
        {
            logger.Error("Error while retrieving an appointment by staff id " + staffId + ": " + e);
            return Enumerable.Empty<Appointment>();
        }
    }

    public async Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId)
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        
            return appointments;
        }
        catch (Exception e)
        {
            logger.Error("Error while retrieving an appointment by customer id " + customerId + ": " + e);
            return Enumerable.Empty<Appointment>();
        }
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
}