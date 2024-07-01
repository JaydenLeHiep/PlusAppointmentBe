using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Services.Interfaces.AppointmentService;

namespace WebApplication1.Controllers.AppointmentController;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString())
        {
            return NotFound(new { message = "You are not authorized to view all businesses." });
        }
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("appointment_id={appointmentId}")]
    public async Task<IActionResult> GetById(int appointmentId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }
        var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        return Ok(appointment);
    }

    [HttpGet("customer/customer_id={customerId}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Customer.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }
        var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customerId);
        return Ok(appointments);
    }

    [HttpGet("business/business_id={businessId}")]
    public async Task<IActionResult> GetByBusinessId(int businessId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        var appointments = await _appointmentService.GetAppointmentsByBusinessIdAsync(businessId);
        return Ok(appointments);
    }

    [HttpGet("staff/staff_id={staffId}")]
    public async Task<IActionResult> GetByStaffId(int staffId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Staff.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        var appointments = await _appointmentService.GetAppointmentsByStaffIdAsync(staffId);
        return Ok(appointments);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddAppointment([FromBody] AppointmentDto appointmentDto)
    {
        try
        {
            // Ensure the provided AppointmentTime is treated as UTC
            appointmentDto.AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc);
            await _appointmentService.AddAppointmentAsync(appointmentDto);
            return Ok(new { message = "Appointment added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("appointment_id={appointmentId}")]
    public async Task<IActionResult> UpdateAppointment(int appointmentId, [FromBody] AppointmentDto appointmentDto)
    {
        try
        {
            // Ensure the provided AppointmentTime is treated as UTC
            appointmentDto.AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc);
            await _appointmentService.UpdateAppointmentAsync(appointmentId, appointmentDto);
            return Ok(new { message = "Appointment updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("appointment_id={appointmentId}")]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        try
        {
            await _appointmentService.DeleteAppointmentAsync(appointmentId);
            return Ok(new { message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
