using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;


namespace PlusAppointment.Services.Interfaces.UserService
{
    public interface IUserService
    {
        Task<IEnumerable<User?>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task RegisterUserAsync(UserRegisterDto userRegisterDto);
        Task UpdateUserAsync(int userId, UserUpdateDto userUpdateDto);
        Task DeleteUserAsync(int id);
        Task<(string? token, string? refreshToken, User? user)> LoginAsync(LoginDto loginDto);
        Task<(string? newAccessToken, string? newRefreshToken)> RefreshTokenAsync(string token, string refreshToken);
    }
}