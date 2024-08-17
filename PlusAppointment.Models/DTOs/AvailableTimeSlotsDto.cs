namespace PlusAppointment.Models.DTOs;

public class AvailableTimeSlotsDto
{
    public int StaffId { get; set; }
    public List<DateTime> AvailableTimeSlots { get; set; } = new List<DateTime>();
}
