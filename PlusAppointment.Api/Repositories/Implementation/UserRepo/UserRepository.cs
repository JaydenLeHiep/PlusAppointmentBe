using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using WebApplication1.Data;
using WebApplication1.Repositories.Interfaces.UserRepo;
using WebApplication1.Utils.Redis;

namespace WebApplication1.Repositories.Implementation.UserRepo
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

            await InvalidateCache();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await InvalidateCache();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await InvalidateCache();
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
            string cacheKey = $"user_usernameOrEmail_{usernameOrEmail}";
            var user = await _redisHelper.GetCacheAsync<User>(cacheKey);
            if (user != null)
            {
                return user;
            }

            user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username or email {usernameOrEmail} not found");
            }

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


        private async Task InvalidateCache()
        {
            await _redisHelper.DeleteKeysByPatternAsync("user_*");
            await _redisHelper.DeleteCacheAsync("all_users");
        }
    }
}