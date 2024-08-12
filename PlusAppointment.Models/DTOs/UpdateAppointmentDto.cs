namespace PlusAppointment.Models.DTOs;

public class UpdateAppointmentDto
{
    public int BusinessId { get; set; }
    public List<ServiceDurationDto> Services { get; set; } // List of services with potential updated durations
    public DateTime AppointmentTime { get; set; }
    public string Comment { get; set; }
}

public class ServiceDurationDto
{
    public int ServiceId { get; set; }
    public TimeSpan? UpdatedDuration { get; set; } // Nullable to check if the duration has been updated
}