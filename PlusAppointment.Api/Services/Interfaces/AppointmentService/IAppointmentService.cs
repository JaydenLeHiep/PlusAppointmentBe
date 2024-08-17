using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Services.Interfaces.AppointmentService
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentRetrieveDto?>> GetAllAppointmentsAsync();
        Task<AppointmentRetrieveDto?> GetAppointmentByIdAsync(int id);
        Task<bool> AddAppointmentAsync(AppointmentDto appointmentDto);
        Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto);
        Task UpdateAppointmentStatusAsync(int id, string status);
        Task DeleteAppointmentAsync(int id);
        Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByBusinessIdAsync(int businessId);
        Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByStaffIdAsync(int staffId);
        Task<IEnumerable<AppointmentRetrieveDto>> GetCustomerAppointmentHistoryAsync(int customerId);
        Task<IEnumerable<DateTime>> GetAvailableTimeSlotsAsync(int staffId, DateTime date);
    }
}