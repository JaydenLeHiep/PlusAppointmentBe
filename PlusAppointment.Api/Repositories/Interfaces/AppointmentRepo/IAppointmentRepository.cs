using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces.AppointmentRepo;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
    Task AddAppointmentAsync(Appointment appointment);
    Task UpdateAppointmentAsync(Appointment appointment);
    Task DeleteAppointmentAsync(int appointmentId);
    Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId);
    Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId);
}