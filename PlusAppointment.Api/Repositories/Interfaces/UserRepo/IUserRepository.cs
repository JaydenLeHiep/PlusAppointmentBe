using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces.UserRepo;

public interface IUserRepository: IRepository<User>
{
    Task<User> GetUserByUsernameAsync(string username);
    
    Task<User> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
}