using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.UserRepo;
using PlusAppointment.Utils.Redis;


namespace PlusAppointment.Repositories.Implementation.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public UserRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<User?>> GetAllAsync()
        {
            const string cacheKey = "all_users";
            var cachedUsers = await _redisHelper.GetCacheAsync<List<User?>>(cacheKey);

            if (cachedUsers != null && cachedUsers.Any())
            {
                return cachedUsers;
            }

            var users = await _context.Users.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, users, TimeSpan.FromMinutes(10));

            return users;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            string cacheKey = $"user_{id}";
            var user = await _redisHelper.GetCacheAsync<User>(cacheKey);
            if (user != null)
            {
                return user;
            }

            user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, user, TimeSpan.FromMinutes(10));
            return user;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await UpdateUserCacheAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await UpdateUserCacheAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await InvalidateUserCacheAsync(user);
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            string cacheKey = $"user_username_{username}";
            var user = await _redisHelper.GetCacheAsync<User>(cacheKey);
            if (user != null)
            {
                return user;
            }

            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return null; // Return null if the user is not found
            }

            await _redisHelper.SetCacheAsync(cacheKey, user, TimeSpan.FromMinutes(10));
            return user;
        }

        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            // Normalize the input to lower case for case-insensitive matching
            string normalizedUsernameOrEmail = usernameOrEmail.ToLowerInvariant();
    
            // Use a cache key that reflects the case-insensitive nature
            string cacheKey = $"user_usernameOrEmail_{normalizedUsernameOrEmail}";
    
            // Check cache first
            var user = await _redisHelper.GetCacheAsync<User>(cacheKey);
            if (user != null)
            {
                return user;
            }

            // Query the database for a case-insensitive match
            user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsernameOrEmail || u.Email.ToLower() == normalizedUsernameOrEmail);
    
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username or email {usernameOrEmail} not found");
            }

            // Store the result in the cache
            await _redisHelper.SetCacheAsync(cacheKey, user, TimeSpan.FromMinutes(10));
    
            return user;
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            string cacheKey = $"user_email_{email}";
            var user = await _redisHelper.GetCacheAsync<User>(cacheKey);
            if (user != null)
            {
                return user;
            }

            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null; // Return null if the user is not found
            }

            await _redisHelper.SetCacheAsync(cacheKey, user, TimeSpan.FromMinutes(10));
            return user;
        }

        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            string cacheKey = $"user_phone_{phone}";
            var user = await _redisHelper.GetCacheAsync<User>(cacheKey);
            if (user != null)
            {
                return user;
            }

            user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
            if (user == null)
            {
                return null; // Return null if the user is not found
            }

            await _redisHelper.SetCacheAsync(cacheKey, user, TimeSpan.FromMinutes(10));
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

        private async Task UpdateUserCacheAsync(User user)
        {
            var userCacheKey = $"user_{user.UserId}";
            await _redisHelper.SetCacheAsync(userCacheKey, user, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<User>(
                "all_users",
                list =>
                {
                    list.RemoveAll(u => u.UserId == user.UserId);
                    list.Add(user);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateUserCacheAsync(User user)
        {
            var userCacheKey = $"user_{user.UserId}";
            await _redisHelper.DeleteCacheAsync(userCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<User>(
                "all_users",
                list =>
                {
                    list.RemoveAll(u => u.UserId == user.UserId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }
    }
}
