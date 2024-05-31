namespace PlusAppointment.Models.DTOs;

public class AppointmentDto
{
    public int CustomerId { get; set; }
    public int BusinessId { get; set; }
    public int ServiceId { get; set; }
    public int StaffId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; }
}