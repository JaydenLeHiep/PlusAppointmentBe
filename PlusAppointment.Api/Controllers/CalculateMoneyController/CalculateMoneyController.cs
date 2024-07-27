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
            var earnings = await _calculateMoneyService.GetDailyEarningsAsync(staffId, request.Commission);
            return Ok(new { DailyEarnings = earnings });
        }

        [HttpPost("weekly/staff_id={staffId}")]
        public async Task<IActionResult> GetWeeklyEarnings(int staffId, [FromBody] CommissionRequest request)
        {
            var earnings = await _calculateMoneyService.GetWeeklyEarningsAsync(staffId, request.Commission);
            return Ok(new { WeeklyEarnings = earnings });
        }

        [HttpPost("monthly/staff_id={staffId}")]
        public async Task<IActionResult> GetMonthlyEarnings(int staffId, [FromBody] CommissionRequest request)
        {
            var earnings = await _calculateMoneyService.GetMonthlyEarningsAsync(staffId, request.Commission);
            return Ok(new { MonthlyEarnings = earnings });
        }
        [HttpPost("daily-specific/staff_id={staffId}")]
        public async Task<IActionResult> GetDailyEarningsForSpecificDate(int staffId, [FromBody] CommissionDateRequest request)
        {
            var earnings = await _calculateMoneyService.GetDailyEarningsForSpecificDateAsync(staffId, request.Commission, request.SpecificDate);
            return Ok(new { DailyEarnings = earnings });
        }
    }

   
}