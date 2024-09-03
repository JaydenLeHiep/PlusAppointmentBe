namespace PlusAppointment.Models.Classes
{
    public class Business
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int UserID { get; set; }
        public User? User { get; set; }
        public ICollection<Service>? Services { get; set; } = new List<Service>();
        public ICollection<Staff>? Staffs { get; set; } = new List<Staff>();
        public ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<NotAvailableDate>? NotAvailableDates { get; set; }
        
        public ICollection<EmailUsage>? EmailUsages { get; set; } = new List<EmailUsage>();

        public Business( string name, string address, string phone, string email, int userID)
        {
            
            Name = name;
            Address = address;
            Phone = phone;
            Email = email;
            UserID = userID;
            
        }
    }
}