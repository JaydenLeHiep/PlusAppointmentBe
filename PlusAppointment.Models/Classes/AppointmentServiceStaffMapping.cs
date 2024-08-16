namespace PlusAppointment.Models.Classes;

public class AppointmentServiceStaffMapping
{
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public int StaffId { get; set; }
    public Staff? Staff { get; set; }

}
