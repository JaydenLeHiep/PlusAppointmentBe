namespace PlusAppointment.Models.Classes.CheckIn;

public class DiscountTier
{
    public int DiscountTierId { get; set; }
    public int BusinessId { get; set; }
    public Business.Business? Business { get; set; }
    public int CheckInThreshold { get; set; }  // Number of check-ins needed for this discount tier
    public decimal DiscountPercentage { get; set; }  // Discount percentage for this tier
}