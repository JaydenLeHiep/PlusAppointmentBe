using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Utils.Hub;


namespace PlusAppointment.Controllers.AppointmentController;

[ApiController]
[Route("api/[controller]")]

public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IHubContext<AppointmentHub> _hubContext;

    public AppointmentsController(IAppointmentService appointmentService, IHubContext<AppointmentHub> hubContext)
    {
        _appointmentService = appointmentService;
        _hubContext = hubContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var tokenType = HttpContext.Request.Headers["Token-Type"].FirstOrDefault();

        if (tokenType == "Access")
        {
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != Role.Admin.ToString())
            {
                return Unauthorized(new { message = "You are not authorized to view all appointments." });
            }
        }
        else
        {
            return Unauthorized(new { message = "Invalid token type for accessing this endpoint." });
        }

        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("appointment_id={appointmentId}")]
    [Authorize]
    public async Task<IActionResult> GetById(int appointmentId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        //var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
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
        //var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Customer.ToString() &&
            userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customerId);
        return Ok(appointments);
    }

    [HttpGet("customer/history/customer_id={customerId}")]
    public async Task<IActionResult> GetCustomerAppointmentHistory(int customerId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Customer.ToString() &&
            userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        var appointments = await _appointmentService.GetCustomerAppointmentHistoryAsync(customerId);

        if (appointments == null || !appointments.Any())
        {
            return NotFound(new { message = "No appointment history found for the customer." });
        }

        return Ok(appointments);
    }


    [HttpGet("business/business_id={businessId}")]
    
    public async Task<IActionResult> GetByBusinessId(int businessId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        //var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
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
        //var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        if (userRole != Role.Admin.ToString() && userRole != Role.Staff.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to view this business." });
        }

        var appointments = await _appointmentService.GetAppointmentsByStaffIdAsync(staffId);
        return Ok(appointments);
    }

    [HttpPost("add-appointment")]
    public async Task<IActionResult> AddAppointment([FromBody] AppointmentDto appointmentDto)
    {
        try
        {
            //appointmentDto.AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc);

            // Add the appointment and send the email
            var appointmentAdded = await _appointmentService.AddAppointmentAsync(appointmentDto);

            if (!appointmentAdded)
            {
                return BadRequest(new { message = "Appointment could not be added because some errors." });
            }
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", "A new appointment has been booked.");



            return Ok(new { message = "Appointment added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("appointment_id={appointmentId}/update-appointment")]
    public async Task<IActionResult> UpdateAppointment(int appointmentId,
        [FromBody] UpdateAppointmentDto updateAppointmentDto)
    {
        try
        {
            // Ensure the provided AppointmentTime is treated as UTC
            //updateAppointmentDto.AppointmentTime = DateTime.SpecifyKind(updateAppointmentDto.AppointmentTime, DateTimeKind.Utc);
            await _appointmentService.UpdateAppointmentAsync(appointmentId, updateAppointmentDto);
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


    [HttpPut("appointment_id={appointmentId}/status-appointment")]
    [Authorize]
    public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId,
        [FromBody] UpdateStatusDto updateStatusDto)
    {
        try
        {
            await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, updateStatusDto.Status);
            // Notify all connected clients about the status change
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentStatusChanged", new { AppointmentId = appointmentId, status = updateStatusDto.Status });
            return Ok(new { message = "Appointment status updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpDelete("appointment_id={appointmentId}/delete-appointment")]
    [Authorize]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        try
        {
            await _appointmentService.DeleteAppointmentAsync(appointmentId);
            // Notify all connected clients about the deletion
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentDeleted", appointmentId);
            return Ok(new { message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("not-available-timeslots")]
    public async Task<IActionResult> GetNotAvailableTimeSlots(int staffId, DateTime date)
    {
        if (staffId <= 0)
        {
            return BadRequest("Invalid staff ID.");
        }

        var notAvailableTimeSlots = await _appointmentService.GetNotAvailableTimeSlotsAsync(staffId, date);

        // Return an empty array if no time slots are found, but still return 200 OK
        if (notAvailableTimeSlots == null || !notAvailableTimeSlots.Any())
        {
            return Ok(new AvailableTimeSlotsDto
            {
                StaffId = staffId,
                AvailableTimeSlots = new List<DateTime>() // Returning an empty list
            });
        }

        // Prepare the response DTO
        var response = new AvailableTimeSlotsDto
        {
            StaffId = staffId,
            AvailableTimeSlots = notAvailableTimeSlots.ToList() // Renamed to "NotAvailableTimeSlots" in the DTO
        };

        return Ok(response);
    }
}