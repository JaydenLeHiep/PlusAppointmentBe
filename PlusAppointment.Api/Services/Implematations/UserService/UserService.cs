using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;




using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.UserRepo;
using WebApplication1.Services.Interfaces.UserService;
namespace WebApplication1.Services.Implematations.UserService;

public class UserService: IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;


    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }


    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task RegisterUserAsync(string username, string password, string email)
    {
        var existingUser = await _userRepository.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            throw new Exception("Username already exists");
        }

        var user = new User
        {
            Username = username,
            Password = HashPassword(password), // Assume HashPassword is a method to hash passwords
            Email = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

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

    public async Task<string> LoginAsync(string usernameOrEmail, string password)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(usernameOrEmail);
        if (user == null || !VerifyPassword(user.Password, password))
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 10000;
    private string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);

        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(key, 0, hashBytes, SaltSize, KeySize);

        return Convert.ToBase64String(hashBytes);
    }
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    private bool VerifyPassword(string hashedPassword, string password)
    {
        var hashBytes = Convert.FromBase64String(hashedPassword);
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(32);

        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != key[i])
            {
                return false;
            }
        }

        return true;
    }
}