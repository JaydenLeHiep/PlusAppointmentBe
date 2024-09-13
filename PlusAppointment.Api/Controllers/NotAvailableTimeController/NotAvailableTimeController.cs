using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.NotAvailableTimeService;

namespace PlusAppointment.Controllers.NotAvailableTimeController
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotAvailableTimeController(INotAvailableTimeService notAvailableTimeService) : ControllerBase
    {
        [HttpGet("business_id={businessId}")]
        public async Task<IActionResult> GetAllByBusiness(int businessId)
        {
            var notAvailableTimes = await notAvailableTimeService.GetAllByBusinessIdAsync(businessId);
            return Ok(notAvailableTimes);
        }

        [HttpGet("business_id={businessId}/staff_id={staffId}")]
        public async Task<IActionResult> GetAllByStaff(int businessId, int staffId)
        {
            var notAvailableTimes = await notAvailableTimeService.GetAllByStaffIdAsync(businessId, staffId);
            return Ok(notAvailableTimes);
        }

        [HttpGet("business_id={businessId}/staff_id={staffId}/notAvailableTime_id={notAvailableTimeId}")]
        public async Task<IActionResult> GetById(int businessId, int staffId, int notAvailableTimeId)
        {
            var notAvailableTime = await notAvailableTimeService.GetByIdAsync(businessId, staffId, notAvailableTimeId);
            if (notAvailableTime == null)
            {
                return Ok(null);
            }
            return Ok(notAvailableTime);
        }

        [Authorize]
        [HttpPost("business_id={businessId}/staff_id={staffId}/add")]
        public async Task<IActionResult> Add(int businessId, int staffId, [FromBody] NotAvailableTimeDto notAvailableTimeDto)
        {
            try
            {
                await notAvailableTimeService.AddNotAvailableTimeAsync(businessId, staffId, notAvailableTimeDto);
                return Ok(new { message = "Not available time added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("business_id={businessId}/staff_id={staffId}/notAvailableTime_id={notAvailableTimeId}")]
        public async Task<IActionResult> Update(int businessId, int staffId, int notAvailableTimeId, [FromBody] NotAvailableTimeDto notAvailableTimeDto)
        {
            try
            {
                await notAvailableTimeService.UpdateNotAvailableTimeAsync(businessId, staffId, notAvailableTimeId, notAvailableTimeDto);
                return Ok(new { message = "Not available time updated successfully" });
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
        [HttpDelete("business_id={businessId}/staff_id={staffId}/notAvailableTime_id={notAvailableTimeId}")]
        public async Task<IActionResult> Delete(int businessId, int staffId, int notAvailableTimeId)
        {
            try
            {
                await notAvailableTimeService.DeleteNotAvailableTimeAsync(businessId, staffId, notAvailableTimeId);
                return Ok(new { message = "Not available time deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}