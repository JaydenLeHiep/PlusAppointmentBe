using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;

public interface IAppointmentReadRepository
{
    Task<List<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
    Task<List<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<List<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId);
    Task<List<Appointment>> GetAppointmentsByStaffIdAsync(int staffId);
    Task<List<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId);
    Task<List<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date);
    Task<Customer?> GetByCustomerIdAsync(int customerId);
    Task<ServiceCategory?> GetServiceCategoryByIdAsync(int categoryId);
}