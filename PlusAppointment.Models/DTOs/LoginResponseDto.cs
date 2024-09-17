namespace PlusAppointment.Models.DTOs;

public class LoginResponseDto
{
    public string? Token { get; set; }
    
    
    public string? Username { get; set; }
    
    public int? UserId { get; set; }
    public string? Role { get; set; }
}