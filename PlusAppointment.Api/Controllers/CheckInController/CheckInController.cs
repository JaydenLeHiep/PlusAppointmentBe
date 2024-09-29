using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Services.Interfaces.CheckInService;

namespace PlusAppointment.Controllers.CheckInController;

[ApiController]
[Route("api/[controller]")]
public class CheckInController: ControllerBase
{
    private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
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
        [HttpGet("checkin_id={checkInId}")]
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
        [HttpGet("business_id={businessId}/checkins")]
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
        [HttpPost("add")]
        public async Task<IActionResult> AddCheckIn([FromBody] CheckIn? checkIn)
        {
            if (checkIn == null)
            {
                return BadRequest(new { message = "CheckIn data cannot be null." });
            }

            try
            {
                await _checkInService.AddCheckInAsync(checkIn);
                return CreatedAtAction(nameof(GetById), new { checkInId = checkIn.CheckInId }, new { message = "CheckIn added successfully.", checkIn });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to add CheckIn: {ex.Message}" });
            }
        }

        // PUT: api/checkin/checkin_id={checkInId}
        [HttpPut("checkin_id={checkInId}")]
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
        [HttpDelete("checkin_id={checkInId}")]
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