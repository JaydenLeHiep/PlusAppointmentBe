using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Services.Interfaces.IOpeningHoursService;

namespace PlusAppointment.Controllers.OpeningHoursController
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpeningHoursController(IOpeningHoursService openingHoursService) : ControllerBase
    {
        [HttpGet("business_id={businessId}")]
        public async Task<IActionResult> GetByBusinessId(int businessId)
        {
            var openingHours = await openingHoursService.GetByBusinessIdAsync(businessId);
            if (openingHours == null)
            {
                return NotFound(new { message = "Opening hours not found for this business." });
            }
            return Ok(openingHours);
        }

        [Authorize]
        [HttpPost("business_id={businessId}/add")]
        public async Task<IActionResult> Add(int businessId, [FromBody] OpeningHours openingHours)
        {
            try
            {
                openingHours.BusinessId = businessId;
                await openingHoursService.AddOpeningHoursAsync(openingHours);
                return Ok(new { message = "Opening hours added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("business_id={businessId}")]
        public async Task<IActionResult> Update(int businessId, [FromBody] OpeningHours openingHours)
        {
            try
            {
                openingHours.BusinessId = businessId;
                await openingHoursService.UpdateOpeningHoursAsync(openingHours);
                return Ok(new { message = "Opening hours updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("business_id={businessId}")]
        public async Task<IActionResult> Delete(int businessId)
        {
            try
            {
                await openingHoursService.DeleteOpeningHoursAsync(businessId);
                return Ok(new { message = "Opening hours deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}