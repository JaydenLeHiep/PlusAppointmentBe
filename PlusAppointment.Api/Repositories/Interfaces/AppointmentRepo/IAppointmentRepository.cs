using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace WebApplication1.Repositories.Interfaces.AppointmentRepo;

public interface IAppointmentRepository
{
    Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
    Task<AppointmentDto> GetAppointmentByIdAsync(int appointmentId);
    Task AddAppointmentAsync(Appointment appointment);
    Task UpdateAppointmentAsync(AppointmentDto appointment);
    Task DeleteAppointmentAsync(int appointmentId);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByBusinessIdAsync(int businessId);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByStaffIdAsync(int staffId);
}