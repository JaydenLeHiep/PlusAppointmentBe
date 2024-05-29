namespace WebApplication1.Models
{
    public class Business
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Staff> Staff { get; set; } = new List<Staff>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<BusinessServices> BusinessServices { get; set; } = new List<BusinessServices>();
    }
}