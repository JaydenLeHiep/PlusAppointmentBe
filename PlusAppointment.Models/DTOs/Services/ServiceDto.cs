namespace PlusAppointment.Models.DTOs;

public class ServiceDto
{
    public int ServiceId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TimeSpan? Duration { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; } // For adding or updating a service
    public string? CategoryName { get; set; } // For returning service data with category name

}