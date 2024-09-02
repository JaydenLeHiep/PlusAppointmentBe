namespace PlusAppointment.Models.Classes;

public class EmailUsage
{
    public int EmailUsageId { get; set; } // Primary key

    public int BusinessId { get; set; } // Foreign key
    public Business? Business { get; set; } // Navigation property

    public int Year { get; set; } // Year of the email usage
    public int Month { get; set; } // Month of the email usage

    public int EmailCount { get; set; } // Number of emails sent
}