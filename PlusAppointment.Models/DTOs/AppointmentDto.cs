namespace PlusAppointment.Models.DTOs;

public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int StaffId { get; set; }
    public string? StaffName { get; set; }
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}