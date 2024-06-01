namespace PlusAppointment.Models.DTOs;

public class UserUpdateDto
{
    public string? Username { get; set; }
    public string? Password { get; set; } // Store hashed passwords
    public string? Email { get; set; }
    public string? Phone { get; set; }
}