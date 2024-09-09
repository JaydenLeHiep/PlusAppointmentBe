using System.Security.Claims;

using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using PlusAppointment.Repositories.Interfaces.UserRepo;
using PlusAppointment.Services.Interfaces.UserService;
using PlusAppointment.Utils.Hash;
using PlusAppointment.Utils.Jwt;


namespace PlusAppointment.Services.Implementations.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHashUtility _hashUtility;

        public UserService(IUserRepository userRepository, IConfiguration configuration, IHashUtility hashUtility)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _hashUtility = hashUtility;
        }

        public async Task<IEnumerable<User?>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            if (string.IsNullOrEmpty(userRegisterDto.Username) || string.IsNullOrEmpty(userRegisterDto.Password) || string.IsNullOrEmpty(userRegisterDto.Email) || string.IsNullOrEmpty(userRegisterDto.Phone))
            {
                throw new Exception("Username, Password, Email, and Phone cannot be null or empty.");
            }

            var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(userRegisterDto.Username);
            if (existingUserByUsername != null)
            {
                throw new Exception("Username already exists");
            }

            // var existingUserByEmail = await _userRepository.GetUserByEmailAsync(userRegisterDto.Email);
            // if (existingUserByEmail != null)
            // {
            //     throw new Exception("Email already exists");
            // }
            //
            // var existingUserByPhone = await _userRepository.GetUserByPhoneAsync(userRegisterDto.Phone);
            // if (existingUserByPhone != null)
            // {
            //     throw new Exception("Phone number already exists");
            // }

            var user = new User 
            (
                username: userRegisterDto.Username,
                password: _hashUtility.HashPassword(userRegisterDto.Password), // Use HashUtility
                email: userRegisterDto.Email,
                phone: userRegisterDto.Phone,
                role: Role.Owner
            );

            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(int userId, UserUpdateDto userUpdateDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            if (!string.IsNullOrEmpty(userUpdateDto.Username))
            {
                existingUser.Username = userUpdateDto.Username;
            }

            if (!string.IsNullOrEmpty(userUpdateDto.Password))
            {
                existingUser.Password = _hashUtility.HashPassword(userUpdateDto.Password); // Ensure you hash the password
            }

            if (!string.IsNullOrEmpty(userUpdateDto.Email))
            {
                existingUser.Email = userUpdateDto.Email;
            }

            if (!string.IsNullOrEmpty(userUpdateDto.Phone))
            {
                existingUser.Phone = userUpdateDto.Phone;
            }

            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task DeleteUserAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
        }
        
        // change to add multiple refresh token



        public async Task<(string? token, string? refreshToken, User? user, string? errorMessage)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameOrEmailAsync(loginDto.UsernameOrEmail);

                if (string.IsNullOrEmpty(user.Password) || !_hashUtility.VerifyPassword(user.Password, loginDto.Password))
                {
                    return (null, null, null, "Invalid password.");
                }

                var token = JwtUtility.GenerateJwtToken(user, _configuration);
                var refreshToken = JwtUtility.GenerateRefreshToken();

                var userRefreshToken = new UserRefreshToken
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    ExpiryTime = DateTime.UtcNow.AddDays(7)
                };

                await _userRepository.AddRefreshTokenAsync(userRefreshToken);

                return (token, refreshToken, user, null);
            }
            catch (KeyNotFoundException)
            {
                return (null, null, null, "User not found.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                return (null, null, null, "An error occurred during login.");
            }
        }



        public async Task<(string? newAccessToken, string? newRefreshToken)> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = JwtUtility.GetPrincipalFromExpiredToken(token, _configuration);
            if (principal == null)
            {
                return (null, null);
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                return (null, null);
            }

            var storedRefreshToken = await _userRepository.GetRefreshTokenAsync(refreshToken);
            if (storedRefreshToken == null || storedRefreshToken.ExpiryTime <= DateTime.UtcNow)
            {
                return (null, null);
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (null, null);
            }

            var newAccessToken = JwtUtility.GenerateJwtToken(user, _configuration);
            var newRefreshToken = JwtUtility.GenerateRefreshToken();

            await _userRepository.DeleteRefreshTokenAsync(storedRefreshToken);

            var newUserRefreshToken = new UserRefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            await _userRepository.AddRefreshTokenAsync(newUserRefreshToken);

            return (newAccessToken, newRefreshToken);
        }
    }
}
