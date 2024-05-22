namespace WebApplication1.Models;

public class Service
{
    public int ServiceId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal Price { get; set; }
    public ICollection<Business> Businesses { get; set; }
    public ICollection<BusinessServices> BusinessServices { get; set; }
}