namespace PlusAppointment.Models.DTOs
{
    public class NotAvailableDateDto
    {
        public int NotAvailableDateId { get; set; }
        public int StaffId { get; set; }
        public int BusinessId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
    }
}