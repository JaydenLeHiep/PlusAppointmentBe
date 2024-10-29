using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Services.Interfaces.EmailUsageService;

namespace PlusAppointment.Controllers.EmailUsageController;

[ApiController]
[Route("api/[controller]")]
public class EmailUsageController : ControllerBase
    {
        private readonly IEmailUsageService _emailUsageService;

        public EmailUsageController(IEmailUsageService emailUsageService)
        {
            _emailUsageService = emailUsageService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllEmailUsages()
        {
            var emailUsages = await _emailUsageService.GetAllEmailUsagesAsync();
            if (!emailUsages.Any())
            {
                return NotFound(new { message = "No email usage records found." });
            }
            return Ok(emailUsages);
        }

        [Authorize]
        [HttpGet("business/{businessId}/year/{year}/month/{month}")]
        public async Task<IActionResult> GetEmailUsagesByBusinessIdAndMonth(int businessId, int year, int month)
        {
            var emailUsages = await _emailUsageService.GetEmailUsagesByBusinessIdAndMonthAsync(businessId, year, month);
            var emailCount = emailUsages.Sum(eu => eu.EmailCount);

            return Ok(new { emailCount });
        }



        [HttpGet("{emailUsageId}")]
        public async Task<IActionResult> GetEmailUsageById(int emailUsageId)
        {
            var emailUsage = await _emailUsageService.GetEmailUsageByIdAsync(emailUsageId);
            if (emailUsage == null)
            {
                return NotFound(new { message = "Email usage record not found." });
            }
            return Ok(emailUsage);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmailUsage emailUsage)
        {
            if (emailUsage == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            await _emailUsageService.AddEmailUsageAsync(emailUsage);
            return Ok(new { message = "Email usage record created successfully." });
        }

        [Authorize]
        [HttpPut("{emailUsageId}")]
        public async Task<IActionResult> Update(int emailUsageId, [FromBody] EmailUsage emailUsage)
        {
            if (emailUsage == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            emailUsage.EmailUsageId = emailUsageId;

            try
            {
                await _emailUsageService.UpdateEmailUsageAsync(emailUsage);
                return Ok(new { message = "Email usage record updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpDelete("{emailUsageId}")]
        public async Task<IActionResult> Delete(int emailUsageId)
        {
            try
            {
                await _emailUsageService.DeleteEmailUsageAsync(emailUsageId);
                return Ok(new { message = "Email usage record deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Delete failed: {ex.Message}" });
            }
        }
    }