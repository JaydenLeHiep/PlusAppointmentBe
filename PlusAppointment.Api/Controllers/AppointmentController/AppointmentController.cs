using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Services.Interfaces.AppointmentService;

namespace WebApplication1.Controllers.AppointmentController;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("appointment_id={id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        return Ok(appointment);
    }

    [HttpGet("customer/customer_id={customerId}")]
    [Authorize]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customerId);
        return Ok(appointments);
    }

    [HttpGet("business/business_id={businessId}")]
    [Authorize]
    public async Task<IActionResult> GetByBusinessId(int businessId)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole != Role.Owner.ToString())
        {
            return Unauthorized(new { message = "User not authorized" });
        }

        var appointments = await _appointmentService.GetAppointmentsByBusinessIdAsync(businessId);
        return Ok(appointments);
    }

    [HttpGet("staff/staff_id={staffId}")]
    [Authorize]
    public async Task<IActionResult> GetByStaffId(int staffId)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole == Role.Customer.ToString() || userRole == null)
        {
            return Unauthorized(new { message = "User not authorized" });
        }

        var appointments = await _appointmentService.GetAppointmentsByStaffIdAsync(staffId);
        return Ok(appointments);
    }

    [HttpPost("add")]
    [Authorize]
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

    [HttpPut("appointment_id={id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentDto appointmentDto)
    {
        try
        {
            // Ensure the provided AppointmentTime is treated as UTC
            appointmentDto.AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc);
            await _appointmentService.UpdateAppointmentAsync(id, appointmentDto);
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

    [HttpDelete("appointment_id={id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        try
        {
            await _appointmentService.DeleteAppointmentAsync(id);
            return Ok(new { message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}