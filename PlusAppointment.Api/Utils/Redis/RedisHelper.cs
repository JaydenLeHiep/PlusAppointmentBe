using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlusAppointment.Utils.Redis
{
    public class RedisHelper
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly JsonSerializerOptions _serializerOptions;

        public RedisHelper(IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");
            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

            _serializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
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

        public async Task UpdateListCacheAsync<T>(string key, Func<List<T>, List<T>> updateFunc,
            TimeSpan? expiry = null) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();
            var cachedData = await db.StringGetAsync(key);

            List<T> list;
            if (!cachedData.IsNullOrEmpty)
            {
                var jsonData = cachedData.ToString();
                list = JsonSerializer.Deserialize<List<T>>(jsonData, _serializerOptions) ?? new List<T>();
            }
            else
            {
                list = new List<T>();
            }

            list = updateFunc(list);

            var serializedData = JsonSerializer.Serialize(list, _serializerOptions);
            await db.StringSetAsync(key, serializedData, expiry);
        }

        public async Task RemoveFromListCacheAsync<T>(string key, Func<List<T>, List<T>> removeFunc,
            TimeSpan? expiry = null) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();
            var cachedData = await db.StringGetAsync(key);

            List<T> list;
            if (!cachedData.IsNullOrEmpty)
            {
                var jsonData = cachedData.ToString();
                list = JsonSerializer.Deserialize<List<T>>(jsonData, _serializerOptions) ?? new List<T>();
            }
            else
            {
                return; // If there's no data in cache, nothing to remove
            }

            list = removeFunc(list);

            var serializedData = JsonSerializer.Serialize(list, _serializerOptions);
            await db.StringSetAsync(key, serializedData, expiry);
        }
    }
}