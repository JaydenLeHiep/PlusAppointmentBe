using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;

public interface IAppointmentReadRepository
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
    Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId);
    Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId);
    Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId);
    Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date);
    Task<Customer?> GetByCustomerIdAsync(int customerId);
    Task<ServiceCategory?> GetServiceCategoryByIdAsync(int categoryId);
}