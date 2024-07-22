using PlusAppointment.Models.DTOs;

namespace WebApplication1.Services.Interfaces.AppointmentService
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto?>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<bool> AddAppointmentAsync(AppointmentDto appointmentDto);
        Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto);
        Task UpdateAppointmentStatusAsync(int id, string status);
        Task DeleteAppointmentAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByBusinessIdAsync(int businessId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByStaffIdAsync(int staffId);
    }
}