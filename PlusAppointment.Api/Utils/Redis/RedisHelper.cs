using StackExchange.Redis;
using System.Text.Json;


namespace WebApplication1.Utils.Redis
{
    public class RedisHelper
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisHelper(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<T?> GetCacheAsync<T>(string key) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();
            var cachedData = await db.StringGetAsync(key);
            if (!cachedData.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            return null;
        }

        public async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serializedData = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serializedData, expiry);
        }

        public async Task DeleteCacheAsync(RedisKey key)
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);
        }

        public async Task DeleteKeysByPatternAsync(string pattern)
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.Configuration);
            var keys = server.Keys(pattern: pattern);

            foreach (var key in keys)
            {
                await DeleteCacheAsync(key);
            }
        }
    }
}