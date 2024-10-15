using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes.Emails;
using PlusAppointment.Services.Interfaces.EmailContentService;

namespace PlusAppointment.Controllers.EmailContentController;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmailContentController : ControllerBase
{
    private readonly IEmailContentService _emailContentService;

    public EmailContentController(IEmailContentService emailContentService)
    {
        _emailContentService = emailContentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var emailContents = await _emailContentService.GetAllAsync();
        return Ok(emailContents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var emailContent = await _emailContentService.GetByIdAsync(id);
        if (emailContent == null)
        {
            return NotFound();
        }
        return Ok(emailContent);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] EmailContent emailContent)
    {
        await _emailContentService.AddAsync(emailContent);
        return CreatedAtAction(nameof(GetById), new { id = emailContent.EmailContentId }, emailContent);
    }

    [HttpPost("add-multiple")]
    public async Task<IActionResult> AddMultiple([FromBody] List<EmailContent> emailContents)
    {
        if (emailContents == null || !emailContents.Any())
        {
            return BadRequest("The list of email contents cannot be empty.");
        }

        await _emailContentService.AddMultipleAsync(emailContents);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailContent emailContent)
    {
        if (id != emailContent.EmailContentId)
        {
            return BadRequest();
        }

        await _emailContentService.UpdateAsync(emailContent);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _emailContentService.DeleteAsync(id);
        return NoContent();
    }
}