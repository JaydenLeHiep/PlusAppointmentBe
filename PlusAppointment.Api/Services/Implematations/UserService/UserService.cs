
using PlusAppointment.Models.Enums;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.UserRepo;
using WebApplication1.Services.Interfaces.UserService;

using WebApplication1.Utils.Hash;
using WebApplication1.Utils.Jwt;

namespace WebApplication1.Services.Implematations.UserService
{
    public class UserService : IUserService
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
                Password = HashUtility.HashPassword(password), // Use HashUtility
                Email = email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow, 
                Role = Role.Owner
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
            if (user == null || !HashUtility.VerifyPassword(user.Password, password))
            {
                return null;
            }

            return JwtUtility.GenerateJwtToken(user, _configuration); // Use JwtUtility
        }
    }
}
