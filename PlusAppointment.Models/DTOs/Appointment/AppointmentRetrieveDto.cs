namespace PlusAppointment.Models.DTOs.Appointment;

public class AppointmentRetrieveDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public int BusinessId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Status { get; set; }
    public string? Comment { get; set; }
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
    public string? StaffName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}