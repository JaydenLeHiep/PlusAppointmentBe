using System.Security.Claims;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Enums;
using PlusAppointment.Repositories.Interfaces.UserRepo;
using PlusAppointment.Services.Interfaces.UserService;
using PlusAppointment.Utils.Hash;
using PlusAppointment.Utils.Jwt;

namespace PlusAppointment.Services.Implementations.UserService;

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

    public async Task<(string? token, string? refreshToken, User? user)> LoginAsync(string usernameOrEmail, string password)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(usernameOrEmail);

        if (user == null || string.IsNullOrEmpty(user.Password) || !HashUtility.VerifyPassword(user.Password, password))
        {
            return (null, null, null);
        }

        var token = JwtUtility.GenerateJwtToken(user, _configuration);
        var refreshToken = JwtUtility.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userRepository.UpdateAsync(user);

        return (token, refreshToken, user);
    }

    public async Task<(string? newAccessToken, string? newRefreshToken)> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = JwtUtility.GetPrincipalFromExpiredToken(token, _configuration);
        if (principal == null)
        {
            return (null, null);
        }

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return (null, null);
        }

        var newAccessToken = JwtUtility.GenerateJwtToken(user, _configuration);
        var newRefreshToken = JwtUtility.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return (newAccessToken, newRefreshToken);
    }

    

}