using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Models.Classes.Business
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
        public ICollection<NotAvailableTime> NotAvailableTimes { get; set; }
        
        public ICollection<ShopPicture>? ShopPictures { get; set; } = new List<ShopPicture>();

        public ICollection<Notification>? Notifications { get; set; } = new List<Notification>();

        public ICollection<EmailUsage>? EmailUsages { get; set; } = new List<EmailUsage>();

        public ICollection<OpeningHours> OpeningHours { get; set; } = new List<OpeningHours>();
        public ICollection<CheckIn.CheckIn> CheckIns { get; set; } = new List<CheckIn.CheckIn>();
        
        public ICollection<DiscountTier> DiscountTiers { get; set; } = new List<DiscountTier>();  // Collection for DiscountTier
        public bool RequiresAppointmentConfirmation { get; set; }
        
        public decimal BirthdayDiscountPercentage { get; set; } = 0;
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