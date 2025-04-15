using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.NotAvailableTimeService;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.NotAvailableTimeController
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotAvailableTimeController : ControllerBase
    {
        private readonly INotAvailableTimeService _notAvailableTimeService;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public NotAvailableTimeController(INotAvailableTimeService notAvailableTimeService, IHubContext<AppointmentHub> hubContext)
        {
            _notAvailableTimeService = notAvailableTimeService;
            _hubContext = hubContext;
        }

        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetAllByBusiness(int businessId)
        {
            var notAvailableTimes = await _notAvailableTimeService.GetAllByBusinessIdAsync(businessId);
            return Ok(notAvailableTimes);
        }

        [HttpGet("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> GetAllByStaff(int businessId, int staffId)
        {
            var notAvailableTimes = await _notAvailableTimeService.GetAllByStaffIdAsync(businessId, staffId);
            return Ok(notAvailableTimes);
        }

        [HttpGet("business/{businessId}/staff/{staffId}/notavailabletime/{notAvailableTimeId}")]
        public async Task<IActionResult> GetById(int businessId, int staffId, int notAvailableTimeId)
        {
            var notAvailableTime = await _notAvailableTimeService.GetByIdAsync(businessId, staffId, notAvailableTimeId);
            if (notAvailableTime == null)
            {
                return NotFound(new { message = "Not available time not found" });
            }
            return Ok(notAvailableTime);
        }

        [Authorize]
        [HttpPost("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> Add(int businessId, int staffId, [FromBody] NotAvailableTimeDto notAvailableTimeDto)
        {
            try
            {
                await _notAvailableTimeService.AddNotAvailableTimeAsync(businessId, staffId, notAvailableTimeDto);
                await _hubContext.Clients.All.SendAsync("ReceiveNotAvailableTimeUpdate", "added");
                return Ok(new { message = "Not available time added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("business/{businessId}/staff/{staffId}/notavailabletime/{notAvailableTimeId}")]
        public async Task<IActionResult> Update(int businessId, int staffId, int notAvailableTimeId, [FromBody] NotAvailableTimeDto notAvailableTimeDto)
        {
            try
            {
                await _notAvailableTimeService.UpdateNotAvailableTimeAsync(businessId, staffId, notAvailableTimeId, notAvailableTimeDto);
                await _hubContext.Clients.All.SendAsync("ReceiveNotAvailableTimeUpdate", "updated");
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
        [HttpDelete("business/{businessId}/staff/{staffId}/notavailabletime/{notAvailableTimeId}")]
        public async Task<IActionResult> Delete(int businessId, int staffId, int notAvailableTimeId)
        {
            try
            {
                await _notAvailableTimeService.DeleteNotAvailableTimeAsync(businessId, staffId, notAvailableTimeId);
                await _hubContext.Clients.All.SendAsync("ReceiveNotAvailableTimeUpdate", "deleted");
                return Ok(new { message = "Not available time deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}