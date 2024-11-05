namespace PlusAppointment.Models.Classes;

public class NotAvailableDate
{
    public int NotAvailableDateId { get; set; }  // Unique ID for each not available date
    public int StaffId { get; set; }  // Foreign key to Staff
    public int BusinessId { get; set; }  // Foreign key to Business
    public DateTime StartDate { get; set; }  // Start of the unavailable period
    public DateTime EndDate { get; set; }  // End of the unavailable period
    public string Reason { get; set; }  // Optional: reason for unavailability

    // Navigation properties
    public Staff Staff { get; set; }
    public Business.Business Business { get; set; }
}