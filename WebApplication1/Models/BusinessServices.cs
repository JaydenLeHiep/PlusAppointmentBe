namespace WebApplication1.Models;

public class BusinessServices
{
    public int BusinessId { get; set; }
    public Business Business { get; set; }

    public int ServiceId { get; set; }
    public Service Service { get; set; }
}