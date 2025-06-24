using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Services.Interfaces.IGoogleReviewService;

namespace PlusAppointment.Controllers.GoogleReviewController
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleReviewsController : ControllerBase
    {
        private readonly IGoogleReviewService _googleReviewService;

        public GoogleReviewsController(IGoogleReviewService googleReviewService)
        {
            _googleReviewService = googleReviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            try
            {
                var result = await _googleReviewService.GetReviewsAsync();
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("avatar")]
        public async Task<IActionResult> GetAvatar([FromQuery] string url)
        {
            try
            {
                var imageBytes = await _googleReviewService.GetAvatarAsync(url);
                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}