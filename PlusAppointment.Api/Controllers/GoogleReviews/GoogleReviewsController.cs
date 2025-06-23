using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PlusAppointment.Controllers.GoogleReviews
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleReviewsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public GoogleReviewsController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            var apiKey = _config["Google:ApiKey"];
            var placeId = _config["Google:PlaceId"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(placeId))
                return BadRequest("API Key or Place ID not configured.");

            var client = _httpClientFactory.CreateClient();
            var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=reviews,rating,user_ratings_total&key={apiKey}";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Google API error: {ex.Message}");
            }
            catch
            {
                return StatusCode(500, "Internal server error while fetching reviews.");
            }
        }
    }
}