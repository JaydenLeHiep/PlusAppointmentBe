namespace PlusAppointment.Models.Classes;

public class AppointmentServiceMapping
{
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }
}
