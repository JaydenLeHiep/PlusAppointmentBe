
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;

namespace PlusAppointment.Utils.SendingSms;

public class SmsReminderJob
{
    private readonly SmsService _smsService;
    private readonly IAppointmentReadRepository _appointmentReadRepository;

    public SmsReminderJob(SmsService smsService, IAppointmentReadRepository appointmentReadRepository)
    {
        _smsService = smsService;
        _appointmentReadRepository = appointmentReadRepository;
    }

    public async Task SendSmsReminderAsync(int appointmentId)
    {
        var appointment = await _appointmentReadRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment == null)
        {
            Console.WriteLine($"Appointment with ID {appointmentId} not found.");
            return;
        }

        var customer = appointment.Customer;
        if (customer == null)
        {
            Console.WriteLine($"Customer for appointment with ID {appointmentId} not found.");
            return;
        }

        var smsMessage = $"Reminder: You have an appointment on {appointment.AppointmentTime}.";
        if (customer.Phone != null) await _smsService.SendSmsAsync(customer.Phone, smsMessage);
    }
}
