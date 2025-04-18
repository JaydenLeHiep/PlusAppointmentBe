namespace PlusAppointment.Models.Classes
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = String.Empty;
        public string? Description { get; set; } = String.Empty;
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public int BusinessId { get; set; } // Foreign key
        public Business.Business? Business { get; set; } // Navigation property
        
        public int? CategoryId { get; set; } // Foreign key
        public ServiceCategory? Category { get; set; } // Navigation property
        public ICollection<AppointmentServiceStaffMapping>? AppointmentServicesStaffs { get; set; }
    }
}