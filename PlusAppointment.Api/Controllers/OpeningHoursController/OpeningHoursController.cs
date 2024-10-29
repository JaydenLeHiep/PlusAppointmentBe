using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Services.Interfaces.IOpeningHoursService;

namespace PlusAppointment.Controllers.OpeningHoursController
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpeningHoursController : ControllerBase
    {
        private readonly IOpeningHoursService _openingHoursService;

        public OpeningHoursController(IOpeningHoursService openingHoursService)
        {
            _openingHoursService = openingHoursService;
        }

        // GET: api/openinghours/business/{businessId}
        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetByBusinessId(int businessId)
        {
            var openingHours = await _openingHoursService.GetByBusinessIdAsync(businessId);
            if (openingHours == null)
            {
                return NotFound(new { message = "Opening hours not found for this business." });
            }
            return Ok(openingHours);
        }

        // POST: api/openinghours/business/{businessId}/add
        [Authorize]
        [HttpPost("business/{businessId}/add")]
        public async Task<IActionResult> Add(int businessId, [FromBody] OpeningHours openingHours)
        {
            if (openingHours == null)
            {
                return BadRequest(new { message = "Opening hours data cannot be null." });
            }

            try
            {
                openingHours.BusinessId = businessId;
                await _openingHoursService.AddOpeningHoursAsync(openingHours);
                return Ok(new { message = "Opening hours added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to add opening hours: {ex.Message}" });
            }
        }

        // PUT: api/openinghours/business/{businessId}
        [Authorize]
        [HttpPut("business/{businessId}")]
        public async Task<IActionResult> Update(int businessId, [FromBody] OpeningHours openingHours)
        {
            if (openingHours == null)
            {
                return BadRequest(new { message = "Opening hours data cannot be null." });
            }

            try
            {
                openingHours.BusinessId = businessId;
                await _openingHoursService.UpdateOpeningHoursAsync(openingHours);
                return Ok(new { message = "Opening hours updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Opening hours not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to update opening hours: {ex.Message}" });
            }
        }

        // DELETE: api/openinghours/business/{businessId}
        [Authorize]
        [HttpDelete("business/{businessId}")]
        public async Task<IActionResult> Delete(int businessId)
        {
            try
            {
                await _openingHoursService.DeleteOpeningHoursAsync(businessId);
                return Ok(new { message = "Opening hours deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Opening hours not found for this business." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to delete opening hours: {ex.Message}" });
            }
        }
    }
}