namespace PlusAppointment.Models.DTOs.Customers;

public class CustomerDto
{
    
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    
    public DateTime? Birthday { get; set; } // New property for birthday
    public bool WantsPromotion { get; set; } // New property for promotion preference
    public string? Note { get; set; }
}