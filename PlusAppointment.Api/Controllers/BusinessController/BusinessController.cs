using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using WebApplication1.Services.Interfaces.BusinessService;

namespace WebApplication1.Controllers.BusinessController;

[ApiController]
[Route("api/[controller]")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var businesses = await _businessService.GetAllBusinessesAsync();
        return Ok(businesses);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = "Business not found" });
        }

        return Ok(business);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] BusinessDto? businessDto)
    {
        if (businessDto == null)
        {
            return BadRequest(new { message = "No data provided." });
        }

        if (string.IsNullOrEmpty(businessDto.Name))
        {
            return BadRequest(new { message = "Business name is required." });
        }

        if (string.IsNullOrEmpty(businessDto.Address))
        {
            return BadRequest(new { message = "Business address is required." });
        }

        if (string.IsNullOrEmpty(businessDto.Phone))
        {
            return BadRequest(new { message = "Business phone is required." });
        }

        if (string.IsNullOrEmpty(businessDto.Email))
        {
            return BadRequest(new { message = "Business email is required." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authorized." });
        }

        var business = new Business
        {
            Name = businessDto.Name,
            Address = businessDto.Address,
            Phone = businessDto.Phone,
            Email = businessDto.Email,
            UserID = int.Parse(userId)
        };

        await _businessService.AddBusinessAsync(business);
        return Ok(new { message = "Business created successfully." });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] BusinessDto? businessDto)
    {
        if (businessDto == null)
        {
            return BadRequest(new { message = "No data provided." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authorized." });
        }

        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = "Business not found." });
        }

        if (business.UserID != int.Parse(userId))
        {
            return Unauthorized(new { message = "User not authorized." });
        }

        if (!string.IsNullOrEmpty(businessDto.Name))
        {
            business.Name = businessDto.Name;
        }

        if (!string.IsNullOrEmpty(businessDto.Address))
        {
            business.Address = businessDto.Address;
        }

        if (!string.IsNullOrEmpty(businessDto.Phone))
        {
            business.Phone = businessDto.Phone;
        }

        if (!string.IsNullOrEmpty(businessDto.Email))
        {
            business.Email = businessDto.Email;
        }

        try
        {
            await _businessService.UpdateBusinessAsync(business);
            return Ok(new { message = "Business updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Update failed: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = "Business not found" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || business.UserID != int.Parse(userId))
        {
            return Unauthorized(new { message = "User not authorized" });
        }

        await _businessService.DeleteBusinessAsync(id);
        return Ok(new { message = "Business deleted successfully" });
    }

    [HttpGet("{id}/services")]
    [Authorize]
    public async Task<IActionResult> GetServices(int id)
    {
        var services = await _businessService.GetServicesByBusinessIdAsync(id);
        if (!services.Any())
        {
            return NotFound(new { message = "No services found for this business" });
        }

        return Ok(services);
    }

    [HttpGet("{id}/staff")]
    [Authorize]
    public async Task<IActionResult> GetStaff(int id)
    {
        var staff = await _businessService.GetStaffByBusinessIdAsync(id);
        if (!staff.Any())
        {
            return NotFound(new { message = "No staff found for this business" });
        }

        return Ok(staff);
    }
}