namespace PlusAppointment.Models.Classes
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? Birthday { get; set; } // New property
        public bool WantsPromotion { get; set; } // New property to track promotion preference
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        // Add BusinessId and Navigation Property
        public int BusinessId { get; set; }
        public Business.Business Business { get; set; }
        
        // Add a collection for check-ins
        public ICollection<CheckIn.CheckIn> CheckIns { get; set; } = new List<CheckIn.CheckIn>();
        public string? Note { get; set; }
    }
}