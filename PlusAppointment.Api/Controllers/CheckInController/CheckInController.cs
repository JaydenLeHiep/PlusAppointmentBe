using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.Classes;
using PlusAppointment.Services.Interfaces.CheckInService;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.CheckInController;

[ApiController]
[Route("api/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;
    private readonly IHubContext<AppointmentHub> _hubContext;

    public CheckInController(ICheckInService checkInService, IHubContext<AppointmentHub> hubContext)
    {
        _checkInService = checkInService;
        _hubContext = hubContext;
    }

    // GET: api/checkin
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var checkIns = await _checkInService.GetAllCheckInsAsync();
        return Ok(checkIns);
    }

    // GET: api/checkin/checkin_id={checkInId}
    [HttpGet("{checkInId}")]
    [Authorize]
    public async Task<IActionResult> GetById(int checkInId)
    {
        var checkIn = await _checkInService.GetCheckInByIdAsync(checkInId);
        if (checkIn == null)
        {
            return NotFound(new { message = "CheckIn not found." });
        }

        return Ok(checkIn);
    }

    // GET: api/checkin/business_id={businessId}
    [HttpGet("business/{businessId}")]
    [Authorize]
    public async Task<IActionResult> GetCheckInsByBusinessId(int businessId)
    {
        var checkIns = await _checkInService.GetCheckInsByBusinessIdAsync(businessId);
        if (!checkIns.Any())
        {
            return NotFound(new { message = "No check-ins found for this business." });
        }

        return Ok(checkIns);
    }

    // POST: api/checkin/add
    [HttpPost]
    public async Task<IActionResult> AddCheckIn([FromBody] CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            return BadRequest(new { message = "CheckIn data cannot be null." });
        }

        try
        {
            await _checkInService.AddCheckInAsync(checkIn);
            await _hubContext.Clients.All.SendAsync("ReceiveNotificationUpdate", "A new notification for the appointment!");
            return CreatedAtAction(nameof(GetById), new { checkInId = checkIn.CheckInId },
                new { message = "CheckIn added successfully.", checkIn });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to add CheckIn: {ex.Message}" });
        }
    }

    // PUT: api/checkin/checkin_id={checkInId}
    [HttpPut("{checkInId}")]
    [Authorize]
    public async Task<IActionResult> UpdateCheckIn(int checkInId, [FromBody] CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            return BadRequest(new { message = "CheckIn data cannot be null." });
        }

        try
        {
            await _checkInService.UpdateCheckInAsync(checkInId, checkIn);
            return Ok(new { message = "CheckIn updated successfully." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "CheckIn not found." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to update CheckIn: {ex.Message}" });
        }
    }

    // DELETE: api/checkin/checkin_id={checkInId}
    [HttpDelete("{checkInId}")]
    [Authorize]
    public async Task<IActionResult> DeleteCheckIn(int checkInId)
    {
        try
        {
            await _checkInService.DeleteCheckInAsync(checkInId);
            return Ok(new { message = "CheckIn deleted successfully." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "CheckIn not found." });
        }
    }
}