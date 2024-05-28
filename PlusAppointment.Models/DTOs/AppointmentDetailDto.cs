namespace PlusAppointment.Models.DTOs;

public class AppointmentDetailDto
{
    public DateTime AppointmentTime { get; set; }
    public string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
}