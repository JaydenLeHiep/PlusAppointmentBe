using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Repositories.Interfaces.AppointmentRepo
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task AddAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentStatusAsync(Appointment appointment);


        Task UpdateAppointmentWithServicesAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto);
        Task DeleteAppointmentAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId);
        Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId);

        Task<bool> IsStaffAvailable(int staffId, DateTime appointmentTime, TimeSpan duration);

        Task<Customer?> GetByCustomerIdAsync(int customerId);

        Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId);
        Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date);
        Task<ServiceCategory?> GetServiceCategoryByIdAsync(int categoryId);
    }
}