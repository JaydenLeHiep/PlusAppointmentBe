namespace PlusAppointment.Models.DTOs;

public class StaffDto
{
    public int BusinessId { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = String.Empty;
    public string Phone { get; set; } = String.Empty;
    public string? Password { get; set; }
}