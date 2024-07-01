namespace PlusAppointment.Models.DTOs;

public class AppointmentCacheDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public TimeSpan ServiceDuration { get; set; }
    public decimal ServicePrice { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string StaffPhone { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = string.Empty;
}