using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces.UserService;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task RegisterUserAsync(string username, string password, string email, string phone);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    
    Task<string?> LoginAsync(string usernameOrEmail, string password);
}