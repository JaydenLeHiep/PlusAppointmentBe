namespace PlusAppointment.Models.DTOs
{
    public class NotAvailableTimeDto
    {
        public int NotAvailableTimeId { get; set; }
        public int StaffId { get; set; }
        public int BusinessId { get; set; }
        public DateTime Date { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Reason { get; set; }
    }
}