using PlusAppointment.Models.Classes;

namespace WebApplication1.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}