namespace WebApplication1.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; } // Store hashed passwords
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Business> Businesses { get; set; }
}
