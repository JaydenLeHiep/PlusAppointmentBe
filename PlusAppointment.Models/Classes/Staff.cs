using PlusAppointment.Models.Interfaces;

namespace PlusAppointment.Models.Classes
{
    public class Staff : IUserIdentity
    {
        public int StaffId { get; set; }
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Phone { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public ICollection<AppointmentServiceStaffMapping>? AppointmentServicesStaffs { get; set; }

        int IUserIdentity.Id => StaffId;
        string? IUserIdentity.Username => Name;
        string IUserIdentity.Role => "Staff"; // Assuming Staff has a fixed role
    }
}