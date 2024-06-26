using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WebApplication1.Utils.Redis
{
    public class RedisHelper
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly JsonSerializerOptions _serializerOptions;

        public RedisHelper(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _serializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };
        }

        public async Task<T?> GetCacheAsync<T>(string key) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();
            var cachedData = await db.StringGetAsync(key);
    
            if (!cachedData.IsNullOrEmpty)
            {
                // Convert RedisValue to string before deserialization
                var jsonData = cachedData.ToString();
        
                // Additional null check to satisfy the compiler
                if (!string.IsNullOrEmpty(jsonData))
                {
                    return JsonSerializer.Deserialize<T>(jsonData, _serializerOptions);
                }
            }
    
            return null;
        }

        public async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serializedData = JsonSerializer.Serialize(value, _serializerOptions);
            await db.StringSetAsync(key, serializedData, expiry);
        }

        public async Task DeleteCacheAsync(string key)
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