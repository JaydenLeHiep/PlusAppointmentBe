namespace WebApplication1.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public int BusinessId { get; set; } // Foreign key
        public Business Business { get; set; } // Navigation property
    }
}