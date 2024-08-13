namespace PlusAppointment.Models.DTOs;

public class CustomerDto
{
    
    public string? Name { get; set; }
    public string Email { get; set; } = String.Empty;
    public string Phone { get; set; } = String.Empty;
}