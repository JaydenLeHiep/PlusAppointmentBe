namespace PlusAppointment.Models.Classes;

public class ServiceCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty; // New property for color

    // Navigation property to Services
    public ICollection<Service>? Services { get; set; }
}