using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.UserRepo;

namespace PlusAppointment.Repositories.Implementation.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User?>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            return user;
        }
        
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
            {
                throw new Exception($"User with ID {user.UserId} not found.");
            }
            existingUser.Password = user.Password;
            existingUser.UpdatedAt = user.UpdatedAt;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            string normalizedUsernameOrEmail = usernameOrEmail.ToLowerInvariant();
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsernameOrEmail || u.Email.ToLower() == normalizedUsernameOrEmail);
    
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username or email {usernameOrEmail} not found");
            }
    
            return user;
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null; // Return null if the user is not found
            }
            
            return user;
        }

        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
            if (user == null)
            {
                return null;
            }
            return user;
        }
        
        // New method to get a refresh token
        public async Task<UserRefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            return await _context.UserRefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        }

        // New method to add a refresh token
        public async Task AddRefreshTokenAsync(UserRefreshToken refreshToken)
        {
            await _context.UserRefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        // New method to delete a specific refresh token
        public async Task DeleteRefreshTokenAsync(UserRefreshToken refreshToken)
        {
            _context.UserRefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }

        // New method to delete all refresh tokens for a user (e.g., on logout from all devices)
        public async Task DeleteAllRefreshTokensForUserAsync(int userId)
        {
            var refreshTokens = _context.UserRefreshTokens.Where(rt => rt.UserId == userId);
            _context.UserRefreshTokens.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();
        }
    }
}
