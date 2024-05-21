namespace WebApplication1.Models.DTOS;

public class AppointmentDetailDto
{
    public DateTime AppointmentTime { get; set; }
    public string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
}