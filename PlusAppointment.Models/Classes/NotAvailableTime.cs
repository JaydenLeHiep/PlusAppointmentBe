namespace PlusAppointment.Models.Classes
{
    public class NotAvailableTime
    {
        public int NotAvailableTimeId { get; set; }
        public int StaffId { get; set; }
        public Staff Staff { get; set; }
        public int BusinessId { get; set; }
        public Business.Business Business { get; set; }
        public DateTime Date { get; set; }
        public DateTime From { get; set; } 
        public DateTime To { get; set; } 

        public string? Reason { get; set; }
    }
}