namespace PlusAppointment.Models.DTOs.Staff;

public class StaffRetrieveDto
{
    public int StaffId { get; set; }
    public int BusinessId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}