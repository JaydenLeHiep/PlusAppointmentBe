namespace PlusAppointment.Models.DTOs;

public class AppointmentRetrieveDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Comment { get; set; } = string.Empty;
    public List<ServiceStaffListsRetrieveDto>? Services { get; set; }
}
public class ServiceStaffListsRetrieveDto
{
    public int ServiceId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TimeSpan? Duration { get; set; }
    public decimal? Price { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
}