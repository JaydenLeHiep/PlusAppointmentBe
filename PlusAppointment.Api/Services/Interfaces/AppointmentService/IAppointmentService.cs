using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Appointment;

namespace PlusAppointment.Services.Interfaces.AppointmentService
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentRetrieveDto?>> GetAllAppointmentsAsync();
        Task<AppointmentRetrieveDto?> GetAppointmentByIdAsync(int id);
        Task<(bool IsSuccess, AppointmentRetrieveDto? Appointment)> AddAppointmentAsync(AppointmentDto appointmentDto);

        Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto);
        Task UpdateAppointmentStatusAsync(int id, string status);
        Task DeleteAppointmentAsync(int id);
        Task DeleteAppointmentForCustomerAsync(int id);
        Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByBusinessIdAsync(int businessId);
        Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByStaffIdAsync(int staffId);
        Task<IEnumerable<AppointmentRetrieveDto>> GetCustomerAppointmentHistoryAsync(int customerId);
        Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date);
    }
}