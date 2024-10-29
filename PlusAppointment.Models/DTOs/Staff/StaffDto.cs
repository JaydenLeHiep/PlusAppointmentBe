namespace PlusAppointment.Models.DTOs.Staff;

public class StaffDto
{
    
    public string? Name { get; set; }
    public string Email { get; set; } = String.Empty;
    public string Phone { get; set; } = String.Empty;
    public string? Password { get; set; }
}