namespace PlusAppointment.Models.Classes
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        // Add BusinessId and Navigation Property
        public int BusinessId { get; set; }
        public Business Business { get; set; }
    }
}