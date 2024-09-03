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
        
        [HttpGet("business_id={businessId}")]
        public async Task<IActionResult> GetAllByBusiness(int businessId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByBusinessIdAsync(businessId);
            // Always return Ok, even if the list is empty
            return Ok(notAvailableDates);
        }
        
        [HttpGet("business_id={businessId}/staff_id={staffId}")]
        public async Task<IActionResult> GetAllByStaff(int businessId, int staffId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByStaffIdAsync(businessId, staffId);
            // Always return Ok, even if the list is empty
            return Ok(notAvailableDates);
        }
        
        [HttpGet("business_id={businessId}/staff_id={staffId}/notAvailable_id={notAvailableId}")]
        public async Task<IActionResult> GetById(int businessId, int staffId, int notAvailableId)
        {
            var notAvailableDate = await _notAvailableDateService.GetByIdAsync(businessId, staffId, notAvailableId);
            if (notAvailableDate == null)
            {
                // If not found, return Ok with null or an empty object
                return Ok(null);
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
        [HttpPut("business_id={businessId}/staff_id={staffId}/notAvailable_id={notAvailableId}")]
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

        [Authorize]
        [HttpDelete("business_id={businessId}/staff_id={staffId}/notAvailable_id={notAvailableId}")]
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