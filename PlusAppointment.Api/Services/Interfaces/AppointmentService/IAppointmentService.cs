using PlusAppointment.Models.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces.AppointmentService;

public interface IAppointmentService
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment> GetAppointmentByIdAsync(int id);
    Task AddAppointmentAsync(AppointmentDto appointmentDto);
    Task UpdateAppointmentAsync(int id, AppointmentDto appointmentDto);
    Task DeleteAppointmentAsync(int id);
    Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId);
    Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId);
}
