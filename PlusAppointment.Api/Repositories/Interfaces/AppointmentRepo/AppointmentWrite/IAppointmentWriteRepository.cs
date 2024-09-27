using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;

public interface IAppointmentWriteRepository
{
    Task AddAppointmentAsync(Appointment appointment);
    Task UpdateAppointmentWithServicesAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto);
    Task UpdateAppointmentStatusAsync(Appointment appointment);
    Task DeleteAppointmentAsync(int appointmentId);
    Task DeleteAppointmentForCustomerAsync(int appointmentId);
}