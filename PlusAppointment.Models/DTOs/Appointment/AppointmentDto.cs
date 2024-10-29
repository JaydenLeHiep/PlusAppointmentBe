namespace PlusAppointment.Models.DTOs.Appointment;

public class AppointmentDto
{
    public int CustomerId { get; set; }
    public int BusinessId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public string? Comment { get; set; }
    public List<ServiceStaffDto> Services { get; set; } = new(); // Modified to include services and staff pairs
}

public class ServiceStaffDto
{
    public int ServiceId { get; set; }
    public int StaffId { get; set; }
}