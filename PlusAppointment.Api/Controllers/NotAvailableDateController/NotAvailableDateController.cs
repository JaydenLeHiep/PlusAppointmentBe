using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.NotAvailableDateService;

namespace PlusAppointment.Controllers.NotAvailableDateController
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotAvailableDateController : ControllerBase
    {
        private readonly INotAvailableDateService _notAvailableDateService;

        public NotAvailableDateController(INotAvailableDateService notAvailableDateService)
        {
            _notAvailableDateService = notAvailableDateService;
        }

        [Authorize]
        [HttpGet("business_id={businessId}/staff_id={staffId}")]
        public async Task<IActionResult> GetAllByStaff(int businessId, int staffId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByStaffIdAsync(businessId, staffId);
            if (!notAvailableDates.Any())
            {
                return NotFound(new { message = "No dates found for this staff." });
            }
            return Ok(notAvailableDates);
        }

        [Authorize]
        [HttpGet("business_id={businessId}/staff_id={staffId}/id={id}")]
        public async Task<IActionResult> GetById(int businessId, int staffId, int id)
        {
            var notAvailableDate = await _notAvailableDateService.GetByIdAsync(businessId, staffId, id);
            if (notAvailableDate == null)
            {
                return NotFound(new { message = "Not available date not found." });
            }
            return Ok(notAvailableDate);
        }

        [Authorize]
        [HttpPost("business_id={businessId}/staff_id={staffId}/add")]
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

        [Authorize]
        [HttpPut("business_id={businessId}/staff_id={staffId}/id={id}")]
        public async Task<IActionResult> Update(int businessId, int staffId, int id, [FromBody] NotAvailableDateDto notAvailableDateDto)
        {
            try
            {
                await _notAvailableDateService.UpdateNotAvailableDateAsync(businessId, staffId, id, notAvailableDateDto);
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

        [Authorize]
        [HttpDelete("business_id={businessId}/staff_id={staffId}/id={id}")]
        public async Task<IActionResult> Delete(int businessId, int staffId, int id)
        {
            try
            {
                await _notAvailableDateService.DeleteNotAvailableDateAsync(businessId, staffId, id);
                return Ok(new { message = "Not available date deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}