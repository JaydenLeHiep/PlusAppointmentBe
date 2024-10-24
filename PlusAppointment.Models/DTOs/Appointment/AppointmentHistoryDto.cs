namespace PlusAppointment.Models.DTOs.Appointment;

public class AppointmentHistoryDto
{
    public DateTime AppointmentTime { get; set; }
    public List<StaffServiceDto> StaffServices { get; set; }
}

public class StaffServiceDto
{
    public string StaffName { get; set; } = String.Empty;
    public string ServiceName { get; set; } = String.Empty;
}