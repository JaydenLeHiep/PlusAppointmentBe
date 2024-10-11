using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes.Emails;
using PlusAppointment.Services.Interfaces.EmailSendingService;

namespace PlusAppointment.Controllers.SendingEmailController;

[ApiController]
[Route("api/[controller]")]
public class SendingEmailController: ControllerBase
{
    private readonly IEmailService _emailService;

    public SendingEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    // Endpoint to send a single email
    [Authorize] 
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailModel? email)
    {
        if (email == null || string.IsNullOrEmpty(email.ToEmail))
        {
            return BadRequest("Email details are not valid.");
        }

        var result = await _emailService.SendEmailAsync(email.ToEmail, email.Subject, email.Body);
        if (result)
        {
            return Ok("Email sent successfully.");
        }

        return StatusCode(500, "Failed to send email.");
    }
    [Authorize] 
    // Endpoint to send bulk emails
    [HttpPost("send-bulk")]
    public async Task<IActionResult> SendBulkEmail([FromBody] BulkEmailModel? bulkEmail)
    {
        if (bulkEmail == null || bulkEmail.ToEmails.Count == 0)
        {
            return BadRequest(new { message = "Bulk email details are not valid." });
        }

        var result = await _emailService.SendBulkEmailAsync(bulkEmail.ToEmails, bulkEmail.Subject, bulkEmail.Body);
        if (result)
        {
            return Ok(new { message = "Bulk email sent successfully." });
        }

        return StatusCode(500, new { message = "Failed to send bulk email." });
    }
}