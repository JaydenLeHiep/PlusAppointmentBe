namespace PlusAppointment.Models.Classes.CheckIn;

public class DiscountCode
{
    public int DiscountCodeId { get; set; }  // Primary key
    public string Code { get; set; }  // Unique code for validation
    public decimal DiscountPercentage { get; set; }  // Discount percentage, e.g., 10% or 20%
    public bool IsUsed { get; set; } = false;  // Tracks if the discount has been used
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}