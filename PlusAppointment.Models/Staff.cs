namespace WebApplication1.Models;

public class Staff
{
    public int StaffId { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Position { get; set; }

}