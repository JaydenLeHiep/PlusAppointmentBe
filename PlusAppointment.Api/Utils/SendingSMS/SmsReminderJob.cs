using PlusAppointment.Repositories.Interfaces.AppointmentRepo;

namespace PlusAppointment.Utils.SendingSms;

public class SmsReminderJob
{
    private readonly SmsService _smsService;
    private readonly IAppointmentRepository _appointmentRepository;

    public SmsReminderJob(SmsService smsService, IAppointmentRepository appointmentRepository)
    {
        _smsService = smsService;
        _appointmentRepository = appointmentRepository;
    }

    public async Task SendSmsReminderAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
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
        await _smsService.SendSmsAsync(customer.Phone, smsMessage);
    }
}
