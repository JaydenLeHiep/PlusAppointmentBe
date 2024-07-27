using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Services.Interfaces.CalculateMoneyService;

namespace PlusAppointment.Controllers.CalculateMoneyController
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalculateMoneyController : ControllerBase
    {
        private readonly ICalculateMoneyService _calculateMoneyService;

        public CalculateMoneyController(ICalculateMoneyService calculateMoneyService)
        {
            _calculateMoneyService = calculateMoneyService;
        }

        [HttpPost("daily/staff_id={staffId}")]
        public async Task<IActionResult> GetDailyEarnings(int staffId, [FromBody] CommissionRequest request)
        {
            var result = await _calculateMoneyService.GetDailyEarningsAsync(staffId, request.Commission);
            if (result.Success)
            {
                return Ok(new { DailyEarnings = result.Earnings });
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }

        [HttpPost("weekly/staff_id={staffId}")]
        public async Task<IActionResult> GetWeeklyEarnings(int staffId, [FromBody] CommissionRequest request)
        {
            var result = await _calculateMoneyService.GetWeeklyEarningsAsync(staffId, request.Commission);
            if (result.Success)
            {
                return Ok(new { WeeklyEarnings = result.Earnings });
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }

        [HttpPost("monthly/staff_id={staffId}")]
        public async Task<IActionResult> GetMonthlyEarnings(int staffId, [FromBody] CommissionRequest request)
        {
            var result = await _calculateMoneyService.GetMonthlyEarningsAsync(staffId, request.Commission);
            if (result.Success)
            {
                return Ok(new { MonthlyEarnings = result.Earnings });
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }

        [HttpPost("daily-specific/staff_id={staffId}")]
        public async Task<IActionResult> GetDailyEarningsForSpecificDate(int staffId, [FromBody] CommissionDateRequest request)
        {
            var result = await _calculateMoneyService.GetDailyEarningsForSpecificDateAsync(staffId, request.Commission, request.SpecificDate);
            if (result.Success)
            {
                return Ok(new { DailyEarnings = result.Earnings });
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }
    }
}
