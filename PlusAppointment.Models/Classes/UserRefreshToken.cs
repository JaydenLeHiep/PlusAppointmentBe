namespace PlusAppointment.Models.Classes;

public class UserRefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryTime { get; set; }

    public User User { get; set; }
}