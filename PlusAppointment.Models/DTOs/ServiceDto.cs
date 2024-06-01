namespace PlusAppointment.Models.DTOs;

public class ServiceDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TimeSpan? Duration { get; set; }
    public decimal? Price { get; set; }

}