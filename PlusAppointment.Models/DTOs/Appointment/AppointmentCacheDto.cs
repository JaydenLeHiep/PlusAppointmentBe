namespace PlusAppointment.Models.DTOs.Appointment;

public class AppointmentCacheDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Updated to reflect the many-to-many relationship
    public List<ServiceStaffCacheDto> ServiceStaffs { get; set; } = new List<ServiceStaffCacheDto>();
}

public class ServiceStaffCacheDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string StaffPhone { get; set; } = string.Empty;
}