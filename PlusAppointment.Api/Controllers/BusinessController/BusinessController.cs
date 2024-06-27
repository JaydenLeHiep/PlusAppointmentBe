using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Services.Interfaces.BusinessService;

namespace WebApplication1.Controllers.BusinessController;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // Ensure all actions require authentication
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString())
        {
            return NotFound(new { message = "You are not authorized to view all businesses." });
        }
        
        var businesses = await _businessService.GetAllBusinessesAsync();
        if (!businesses.Any())
        {
            return NotFound(new { message = "No businesses found." });
        }
        return Ok(businesses);
    }

    [HttpGet("business_id={id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = "Business not found." });
        }

        if (userRole != Role.Admin.ToString() && business.UserID != currentUserId)
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        return Ok(business);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BusinessDto? businessDto)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to create a business." });
        }

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
        (
            name: businessDto.Name,
            address: businessDto.Address,
            phone: businessDto.Phone,
            email: businessDto.Email,
            userID: int.Parse(userId)
        );

        await _businessService.AddBusinessAsync(business);
        return Ok(new { message = "Business created successfully." });
    }

    [HttpPut("business_id={id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BusinessDto? businessDto)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to update this business." });
        }

        if (businessDto == null)
        {
            return BadRequest(new { message = "No data provided." });
        }

        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = "Business not found." });
        }

        if (business.UserID != currentUserId && userRole != Role.Admin.ToString())
        {
            return NotFound(new { message = "You are not authorized to update this business." });
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

    [HttpDelete("business_id={id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to delete this business." });
        }

        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = "Business not found." });
        }

        if (business.UserID != currentUserId && userRole != Role.Admin.ToString())
        {
            return NotFound(new { message = "You are not authorized to delete this business." });
        }

        await _businessService.DeleteBusinessAsync(id);
        return Ok(new { message = "Business deleted successfully." });
    }

    [HttpGet("business_id={id}/services")]
    public async Task<IActionResult> GetServices(int id)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view the services of this business." });
        }

        var services = await _businessService.GetServicesByBusinessIdAsync(id);
        if (!services.Any())
        {
            return NotFound(new { message = "No services found for this business." });
        }

        return Ok(services);
    }

    [HttpGet("business_id={id}/staff")]
    public async Task<IActionResult> GetStaff(int id)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view the staff of this business." });
        }

        var staff = await _businessService.GetStaffByBusinessIdAsync(id);
        if (!staff.Any())
        {
            return NotFound(new { message = "No staff found for this business." });
        }

        return Ok(staff);
    }
}
