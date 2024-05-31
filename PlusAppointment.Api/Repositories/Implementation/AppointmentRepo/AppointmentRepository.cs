using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.DTOs;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;

namespace WebApplication1.Repositories.Implementation.AppointmentRepo;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .ToListAsync();

        return appointments.Select(a => new AppointmentDto
        {
            AppointmentId = a.AppointmentId,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.Name,
            BusinessId = a.BusinessId,
            BusinessName = a.Business.Name,
            ServiceId = a.ServiceId,
            ServiceName = a.Service.Name,
            StaffId = a.StaffId,
            StaffName = a.Staff.Name,
            AppointmentTime = a.AppointmentTime,
            Duration = a.Duration,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();
    }

    public async Task<AppointmentDto> GetAppointmentByIdAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (appointment == null)
        {
            return null; // Or throw an appropriate exception
        }

        return new AppointmentDto
        {
            AppointmentId = appointment.AppointmentId,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer.Name,
            BusinessId = appointment.BusinessId,
            BusinessName = appointment.Business.Name,
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service.Name,
            StaffId = appointment.StaffId,
            StaffName = appointment.Staff.Name,
            AppointmentTime = appointment.AppointmentTime,
            Duration = appointment.Duration,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }

    public async Task AddAppointmentAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAppointmentAsync(AppointmentDto appointment)
    {
        _context.Entry(appointment).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAppointmentAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();

        return appointments.Select(a => new AppointmentDto
        {
            AppointmentId = a.AppointmentId,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.Name,
            BusinessId = a.BusinessId,
            BusinessName = a.Business.Name,
            ServiceId = a.ServiceId,
            ServiceName = a.Service.Name,
            StaffId = a.StaffId,
            StaffName = a.Staff.Name,
            AppointmentTime = a.AppointmentTime,
            Duration = a.Duration,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByBusinessIdAsync(int businessId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.BusinessId == businessId)
            .ToListAsync();

        return appointments.Select(a => new AppointmentDto
        {
            AppointmentId = a.AppointmentId,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.Name,
            BusinessId = a.BusinessId,
            BusinessName = a.Business.Name,
            ServiceId = a.ServiceId,
            ServiceName = a.Service.Name,
            StaffId = a.StaffId,
            StaffName = a.Staff.Name,
            AppointmentTime = a.AppointmentTime,
            Duration = a.Duration,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByStaffIdAsync(int staffId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Business)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.StaffId == staffId)
            .ToListAsync();

        return appointments.Select(a => new AppointmentDto
        {
            AppointmentId = a.AppointmentId,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.Name,
            BusinessId = a.BusinessId,
            BusinessName = a.Business.Name,
            ServiceId = a.ServiceId,
            ServiceName = a.Service.Name,
            StaffId = a.StaffId,
            StaffName = a.Staff.Name,
            AppointmentTime = a.AppointmentTime,
            Duration = a.Duration,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();
    }
}