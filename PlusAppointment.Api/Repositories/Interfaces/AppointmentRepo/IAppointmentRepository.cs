using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace WebApplication1.Repositories.Interfaces.AppointmentRepo
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task AddAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentStatusAsync(Appointment appointment);
        Task UpdateAppointmentServicesMappingAsync(int appointmentId, List<Service> validServices);

        Task UpdateAppointmentWithServicesAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto,
            List<Service> validServices, TimeSpan totalDuration);
        Task DeleteAppointmentAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId);
        Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId);

        Task<bool> IsStaffAvailable(int staffId, DateTime appointmentTime, TimeSpan duration);
        
        Task<Customer?> GetByCustomerIdAsync(int customerId);
    }
}