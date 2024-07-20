namespace PlusAppointment.Models.DTOs;

public class UpdateAppointmentDto
{
    public int BusinessId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public List<int> ServiceIds { get; set; } = new();
    public string? Comment { get; set; }
}