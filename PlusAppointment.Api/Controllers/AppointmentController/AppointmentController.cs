using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Appointment;
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

    [HttpGet("{appointmentId}")]
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

    [HttpGet("customers/{customerId}/appointments")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customerId);
        if (appointments == null || !appointments.Any())
        {
            return Ok(new { message = "No appointments were found for this customer." });
        }
        return Ok(appointments);
    }

    [HttpGet("customers/{customerId}/appointment-history")]
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


    [HttpGet("businesses/{businessId}/appointments")]
    
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

    [HttpGet("staff/{staffId}/appointments")]
    
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

    [HttpPost]
    public async Task<IActionResult> AddAppointment([FromBody] AppointmentDto appointmentDto)
    {
        try
        {
            // Add the appointment and get the result (success flag and appointment object)
            var (isSuccess, appointment) = await _appointmentService.AddAppointmentAsync(appointmentDto);

            if (!isSuccess)
            {
                // If the appointment was not added, return a BadRequest with an appropriate message
                return BadRequest(new { message = "Appointment could not be added due to some errors." });
            }

            // Send both a message and the appointment details via SignalR
            // Call the SendAppointmentUpdate method on the Hub
            // Send both a message and the appointment details via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", new 
            {
                message = "A new appointment has been booked.",
                appointment = appointment
            });

            // Optionally send a notification update
            await _hubContext.Clients.All.SendAsync("ReceiveNotificationUpdate", "A new notification for the appointment!");

            // Return a success response with the added appointment details
            return Ok(new 
            { 
                message = "Appointment added successfully", 
                appointment = appointment 
            });
        }
        catch (Exception ex)
        {
            // Return a BadRequest with the exception message in case of any errors
            return BadRequest(new { message = ex.Message });
        }
    }



    [HttpPut("{appointmentId}")]
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


    [HttpPut("{appointmentId}/status")]
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


    [HttpDelete("{appointmentId}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        try
        {
            await _appointmentService.DeleteAppointmentAsync(appointmentId);
            // Notify all connected clients about the deletion
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentDeleted", appointmentId);
            // Optionally send a notification update
            await _hubContext.Clients.All.SendAsync("ReceiveNotificationUpdate", "A new notification for the appointment!");
            return Ok(new { message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("staff/{staffId}/not-available-timeslots")]
    public async Task<IActionResult> GetNotAvailableTimeSlots(int staffId, [FromQuery] DateTime date)
    {
        if (staffId <= 0)
        {
            return BadRequest("Invalid staff ID.");
        }

        // Call the service to get the not-available time slots
        var notAvailableTimeSlots = await _appointmentService.GetNotAvailableTimeSlotsAsync(staffId, date);

        // Check if the time slots exist, return empty if none found
        if (notAvailableTimeSlots == null || !notAvailableTimeSlots.Any())
        {
            return Ok(new AvailableTimeSlotsDto
            {
                StaffId = staffId,
                AvailableTimeSlots = new List<DateTime>() // Return an empty list if no slots are found
            });
        }

        // Prepare the response DTO with the available slots
        var response = new AvailableTimeSlotsDto
        {
            StaffId = staffId,
            AvailableTimeSlots = notAvailableTimeSlots.ToList() // Convert to list for the DTO
        };

        // Return the 200 OK response with the populated response DTO
        return Ok(response);
    }
    
    [HttpDelete("{appointmentId}/customer")]
    public async Task<IActionResult> DeleteAppointmentForCustomer(int appointmentId)
    {
        try
        {
            await _appointmentService.DeleteAppointmentForCustomerAsync(appointmentId);
            // Notify all connected clients about the deletion
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentForCustomerDeleted", appointmentId);
            await _hubContext.Clients.All.SendAsync("ReceiveNotificationUpdate", "A new notification for the appointment!");
            return Ok(new { message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}