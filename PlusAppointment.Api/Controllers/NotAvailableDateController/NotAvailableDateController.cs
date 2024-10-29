using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.NotAvailableDateService;

namespace PlusAppointment.Controllers
{
    [ApiController]
    [Route("api/notavailabledate")]
    public class NotAvailableDateController : ControllerBase
    {
        private readonly INotAvailableDateService _notAvailableDateService;

        public NotAvailableDateController(INotAvailableDateService notAvailableDateService)
        {
            _notAvailableDateService = notAvailableDateService;
        }

        // GET: api/notavailabledate/business/{businessId}
        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetAllByBusiness(int businessId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByBusinessIdAsync(businessId);
            return Ok(notAvailableDates); // Always return Ok, even if empty
        }

        // GET: api/notavailabledate/business/{businessId}/staff/{staffId}
        [HttpGet("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> GetAllByStaff(int businessId, int staffId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByStaffIdAsync(businessId, staffId);
            return Ok(notAvailableDates); // Always return Ok, even if empty
        }

        // GET: api/notavailabledate/business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}
        [HttpGet("business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}")]
        public async Task<IActionResult> GetById(int businessId, int staffId, int notAvailableId)
        {
            var notAvailableDate = await _notAvailableDateService.GetByIdAsync(businessId, staffId, notAvailableId);
            if (notAvailableDate == null)
            {
                return Ok(null); // Return Ok with null if not found
            }
            return Ok(notAvailableDate);
        }

        // POST: api/notavailabledate/business/{businessId}/staff/{staffId}
        [Authorize]
        [HttpPost("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> Add(int businessId, int staffId, [FromBody] NotAvailableDateDto notAvailableDateDto)
        {
            try
            {
                await _notAvailableDateService.AddNotAvailableDateAsync(businessId, staffId, notAvailableDateDto);
                return Ok(new { message = "Not available date added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/notavailabledate/business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}
        [Authorize]
        [HttpPut("business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}")]
        public async Task<IActionResult> Update(int businessId, int staffId, int notAvailableId, [FromBody] NotAvailableDateDto notAvailableDateDto)
        {
            try
            {
                await _notAvailableDateService.UpdateNotAvailableDateAsync(businessId, staffId, notAvailableId, notAvailableDateDto);
                return Ok(new { message = "Not available date updated successfully" });
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

        // DELETE: api/notavailabledate/business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}
        [Authorize]
        [HttpDelete("business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}")]
        public async Task<IActionResult> Delete(int businessId, int staffId, int notAvailableId)
        {
            try
            {
                await _notAvailableDateService.DeleteNotAvailableDateAsync(businessId, staffId, notAvailableId);
                return Ok(new { message = "Not available date deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}