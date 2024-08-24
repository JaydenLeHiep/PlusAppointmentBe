namespace PlusAppointment.Models.Classes
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int BusinessId { get; set; }
        public Business? Business { get; set; }

        public DateTime AppointmentTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Comment { get; set; }
        public ICollection<AppointmentServiceStaffMapping>? AppointmentServices { get; set; }
    }

}