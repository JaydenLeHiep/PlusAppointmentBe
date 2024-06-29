using PlusAppointment.Models.Classes;

namespace WebApplication1.Repositories.Interfaces.UserRepo;

public interface IUserRepository: IRepository<User>
{
    
    
    Task<User?> GetUserByUsernameAsync(string username);
    
    Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByPhoneAsync(string phone);

    
}