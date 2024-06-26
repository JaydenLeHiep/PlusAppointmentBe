using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlusAppointment.Models.Classes;

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
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                Converters =
                {
                    new BusinessCollectionConverter()
                }
            };
        }

        public async Task<T?> GetCacheAsync<T>(string key) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();
            var cachedData = await db.StringGetAsync(key);
    
            if (!cachedData.IsNullOrEmpty)
            {
                var jsonData = cachedData.ToString();
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

    public class BusinessCollectionConverter : JsonConverter<ICollection<Business>>
    {
        public override ICollection<Business>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var businesses = JsonSerializer.Deserialize<List<Business>>(ref reader, options);
            return businesses ?? new List<Business>();
        }

        public override void Write(Utf8JsonWriter writer, ICollection<Business> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
