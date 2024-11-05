

namespace PlusAppointment.Models.DTOs.CheckIn;

public class DiscountTierDto
{

    public int CheckInThreshold { get; set; }  // Number of check-ins needed for this discount tier
    public decimal DiscountPercentage { get; set; }  // Discount percentage for this tier
}