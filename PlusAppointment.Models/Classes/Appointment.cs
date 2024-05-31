namespace WebApplication1.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int BusinessId { get; set; }
        public Business Business { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int StaffId { get; set; }
        public Staff Staff { get; set; }
        public DateTime AppointmentTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}