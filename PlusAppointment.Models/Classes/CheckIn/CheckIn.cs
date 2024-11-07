using PlusAppointment.Models.Enums;

namespace PlusAppointment.Models.Classes.CheckIn;

public class CheckIn
{
    public int CheckInId { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int BusinessId { get; set; }
    public Business.Business? Business { get; set; }
    public DateTime CheckInTime { get; set; }
    public CheckInType CheckInType { get; set; }  // Enum property to indicate walk-in or online check-in
    

}