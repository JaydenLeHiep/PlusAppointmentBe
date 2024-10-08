public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Comment { get; set; }
    public List<ServiceStaffDto> Services { get; set; } = new(); // Modified to include services and staff pairs
}

public class ServiceStaffDto
{
    public int ServiceId { get; set; }
    public int StaffId { get; set; }
}