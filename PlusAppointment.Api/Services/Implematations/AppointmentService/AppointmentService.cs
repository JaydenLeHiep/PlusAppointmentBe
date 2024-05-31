using PlusAppointment.Models.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;
using WebApplication1.Repositories.Interfaces.BusinessRepo;
using WebApplication1.Services.Interfaces.AppointmentService;
using WebApplication1.Services.Interfaces.BusinessService;

namespace WebApplication1.Services.Implematations.AppointmentService;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    public readonly IBusinessRepository _businessRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository, IBusinessRepository businessRepository)
    {
        _appointmentRepository = appointmentRepository;
        _businessRepository = businessRepository;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
    {
        return await _appointmentRepository.GetAllAppointmentsAsync();
    }

    public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
    {
        return await _appointmentRepository.GetAppointmentByIdAsync(id);
    }

    public async Task AddAppointmentAsync(AppointmentDto appointmentDto)
    {
        // Validate the BusinessId
        var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
        if (business == null)
        {
            throw new ArgumentException("Invalid BusinessId");
        }

        // Validate the ServiceId
        var services = await _businessRepository.GetServicesByBusinessIdAsync(appointmentDto.BusinessId);
        if (!services.Any(s => s.ServiceId == appointmentDto.ServiceId))
        {
            throw new ArgumentException("Invalid ServiceId for the given BusinessId");
        }

        // Validate the StaffId
        var staff = await _businessRepository.GetStaffByBusinessIdAsync(appointmentDto.BusinessId);
        if (!staff.Any(s => s.StaffId == appointmentDto.StaffId))
        {
            throw new ArgumentException("Invalid StaffId for the given BusinessId");
        }

        var appointment = new Appointment
        {
            CustomerId = appointmentDto.CustomerId,
            BusinessId = appointmentDto.BusinessId,
            ServiceId = appointmentDto.ServiceId,
            StaffId = appointmentDto.StaffId,
            AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc),
            Duration = appointmentDto.Duration,
            Status = appointmentDto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _appointmentRepository.AddAppointmentAsync(appointment);
    }

    public async Task UpdateAppointmentAsync(int id, AppointmentDto appointmentDto)
    {
        // Validate the BusinessId
        var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
        if (business == null)
        {
            throw new ArgumentException("Invalid BusinessId");
        }

        // Validate the ServiceId
        var services = await _businessRepository.GetServicesByBusinessIdAsync(appointmentDto.BusinessId);
        if (!services.Any(s => s.ServiceId == appointmentDto.ServiceId))
        {
            throw new ArgumentException("Invalid ServiceId for the given BusinessId");
        }

        // Validate the StaffId
        var staff = await _businessRepository.GetStaffByBusinessIdAsync(appointmentDto.BusinessId);
        if (!staff.Any(s => s.StaffId == appointmentDto.StaffId))
        {
            throw new ArgumentException("Invalid StaffId for the given BusinessId");
        }

        var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            throw new KeyNotFoundException("Appointment not found");
        }

        appointment.CustomerId = appointmentDto.CustomerId;
        appointment.BusinessId = appointmentDto.BusinessId;
        appointment.ServiceId = appointmentDto.ServiceId;
        appointment.StaffId = appointmentDto.StaffId;
        appointment.AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc);
        appointment.Duration = appointmentDto.Duration;
        appointment.Status = appointmentDto.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAppointmentAsync(appointment);
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        await _appointmentRepository.DeleteAppointmentAsync(id);
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId)
    {
        return await _appointmentRepository.GetAppointmentsByCustomerIdAsync(customerId);
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByBusinessIdAsync(int businessId)
    {
        return await _appointmentRepository.GetAppointmentsByBusinessIdAsync(businessId);
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByStaffIdAsync(int staffId)
    {
        return await _appointmentRepository.GetAppointmentsByStaffIdAsync(staffId);
    }
}