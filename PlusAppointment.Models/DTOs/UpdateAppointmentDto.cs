namespace PlusAppointment.Models.DTOs
{
    public class UpdateAppointmentDto
    {
        
        public List<ServiceStaffDurationDto> Services { get; set; } = new(); // List of services with potential updated durations and associated staff
        public DateTime AppointmentTime { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class ServiceStaffDurationDto
    {
        public int ServiceId { get; set; }
        public int StaffId { get; set; } // Associated staff member for the service
        public TimeSpan? UpdatedDuration { get; set; } // Nullable to check if the duration has been updated
    }
}