using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.NotAvailableDateService;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers
{
    [ApiController]
    [Route("api/notavailabledate")]
    public class NotAvailableDateController : ControllerBase
    {
        private readonly INotAvailableDateService _notAvailableDateService;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public NotAvailableDateController(INotAvailableDateService notAvailableDateService, IHubContext<AppointmentHub> hubContext)
        {
            _notAvailableDateService = notAvailableDateService;
            _hubContext = hubContext;
        }

        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetAllByBusiness(int businessId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByBusinessIdAsync(businessId);
            return Ok(notAvailableDates);
        }

        [HttpGet("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> GetAllByStaff(int businessId, int staffId)
        {
            var notAvailableDates = await _notAvailableDateService.GetAllByStaffIdAsync(businessId, staffId);
            return Ok(notAvailableDates);
        }

        [HttpGet("business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}")]
        public async Task<IActionResult> GetById(int businessId, int staffId, int notAvailableId)
        {
            var notAvailableDate = await _notAvailableDateService.GetByIdAsync(businessId, staffId, notAvailableId);
            if (notAvailableDate == null)
            {
                return Ok(null);
            }
            return Ok(notAvailableDate);
        }

        [Authorize]
        [HttpPost("business/{businessId}/staff/{staffId}")]
        public async Task<IActionResult> Add(int businessId, int staffId, [FromBody] NotAvailableDateDto notAvailableDateDto)
        {
            try
            {
                await _notAvailableDateService.AddNotAvailableDateAsync(businessId, staffId, notAvailableDateDto);
                await _hubContext.Clients.All.SendAsync("ReceiveNotAvailableDateUpdate", "added");
                return Ok(new { message = "Not available date added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}")]
        public async Task<IActionResult> Update(int businessId, int staffId, int notAvailableId, [FromBody] NotAvailableDateDto notAvailableDateDto)
        {
            try
            {
                await _notAvailableDateService.UpdateNotAvailableDateAsync(businessId, staffId, notAvailableId, notAvailableDateDto);
                await _hubContext.Clients.All.SendAsync("ReceiveNotAvailableDateUpdate", "updated");
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
        [HttpDelete("business/{businessId}/staff/{staffId}/notavailable/{notAvailableId}")]
        public async Task<IActionResult> Delete(int businessId, int staffId, int notAvailableId)
        {
            try
            {
                await _notAvailableDateService.DeleteNotAvailableDateAsync(businessId, staffId, notAvailableId);
                await _hubContext.Clients.All.SendAsync("ReceiveNotAvailableDateUpdate", "deleted");
                return Ok(new { message = "Not available date deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}