using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using PlusAppointment.Services.Interfaces.IGoogleReviewService;
using PlusAppointment.Utils.Redis;
using System.Net.Http;

namespace PlusAppointment.Services.Implementations.GoogleService
{
    public class GoogleReviewService : IGoogleReviewService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly RedisHelper _redisHelper;

        public GoogleReviewService(IHttpClientFactory httpClientFactory, IConfiguration config, RedisHelper redisHelper)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _redisHelper = redisHelper;
        }

        public async Task<string> GetReviewsAsync()
        {
            var apiKey = _config["Google:ApiKey"];
            var placeId = _config["Google:PlaceId"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(placeId))
                throw new ArgumentException("API Key or Place ID not configured.");

            string cacheKey = "google_reviews";
            var cached = await _redisHelper.GetCacheAsync<string>(cacheKey);

            if (!string.IsNullOrEmpty(cached))
            {
                Console.WriteLine("Serving Google Reviews from Redis cache.");
                return cached;
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=reviews,rating,user_ratings_total&key={apiKey}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            await _redisHelper.SetCacheAsync(cacheKey, content, TimeSpan.FromDays(30));

            Console.WriteLine("Fetched new Google Reviews and saved to Redis.");
            return content;
        }

        public async Task<byte[]> GetAvatarAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Missing avatar URL.");

            var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(url)))
                .Replace("/", "_").Replace("+", "-");
            var cacheKey = $"google_avatar_{hash}";

            var cached = await _redisHelper.GetCacheAsync<byte[]>(cacheKey);
            if (cached != null)
                return cached;

            var client = _httpClientFactory.CreateClient();
            var imageBytes = await client.GetByteArrayAsync(url);

            await _redisHelper.SetCacheAsync(cacheKey, imageBytes, TimeSpan.FromDays(30));
            return imageBytes;
        }
    }
}