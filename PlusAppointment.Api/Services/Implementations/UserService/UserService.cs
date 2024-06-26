using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Enums;
using WebApplication1.Repositories.Interfaces.UserRepo;
using WebApplication1.Services.Interfaces.UserService;
using WebApplication1.Utils.Hash;
using WebApplication1.Utils.Jwt;

namespace WebApplication1.Services.Implementations.UserService;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<IEnumerable<User?>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task RegisterUserAsync(string username, string password, string email, string phone)
    {
        var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(username);
        if (existingUserByUsername != null)
        {
            throw new Exception("Username already exists");
        }

        var existingUserByEmail = await _userRepository.GetUserByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            throw new Exception("Email already exists");
        }

        var existingUserByPhone = await _userRepository.GetUserByPhoneAsync(phone);
        if (existingUserByPhone != null)
        {
            throw new Exception("Phone number already exists");
        }

        var user = new User 
        (
                
            username: username,
            password: HashUtility.HashPassword(password), // Use HashUtility
            email: email,
            phone: phone,
            role: Role.Owner
        );

        await _userRepository.AddAsync(user);
    }

    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(int id)
    {
        await _userRepository.DeleteAsync(id);
    }

    public async Task<(string? token, User? user)> LoginAsync(string usernameOrEmail, string password)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(usernameOrEmail);

        // Check if the user or the user's password is null
        if (user == null || string.IsNullOrEmpty(user.Password) || !HashUtility.VerifyPassword(user.Password, password))
        {
            return (null, null);
        }

        var token = JwtUtility.GenerateJwtToken(user, _configuration);
        return (token, user); // Return the token and user
    }

}