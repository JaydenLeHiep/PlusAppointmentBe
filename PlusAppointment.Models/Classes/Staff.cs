using PlusAppointment.Models.Interfaces;

namespace PlusAppointment.Models.Classes
{
    public class Staff : IUserIdentity
    {
        public int StaffId { get; set; }
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        int IUserIdentity.Id => StaffId;
        string IUserIdentity.Username => Name;
        string IUserIdentity.Role => "Staff"; // Assuming Staff has a fixed role
    }
}