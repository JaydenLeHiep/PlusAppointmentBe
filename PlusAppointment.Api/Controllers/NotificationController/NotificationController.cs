using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;

using PlusAppointment.Models.DTOs.Notifications;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.NotificationService;

namespace PlusAppointment.Controllers.NotificationController;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    [Authorize]
    [HttpGet("business/{businessId}/all")]
    public async Task<IActionResult> GetAllNotifications(int businessId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to update this staff." });
        }


        var notifications = await _notificationService.GetAllNotificationsByBusinessIdAsync(businessId);



        return Ok(notifications);
    }

    // GET: api/notification/{businessId}
    // GET: api/notification/business/{businessId}
    [Authorize]
    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetNotifications(int businessId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to update this staff." });
        }


        var notifications = await _notificationService.GetNotificationsByBusinessIdAsync(businessId);



        return Ok(notifications);
    }


    // POST: api/notification
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequestDto? request)
    {
        if (request == null || !Enum.IsDefined(typeof(NotificationType), request.NotificationType))
        {
            return BadRequest("Invalid notification type");
        }

        try
        {
            await _notificationService.AddNotificationAsync(request.BusinessId, request.Message,
                request.NotificationType);
            return Ok(new { message = "Notification created successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { message = $"An error occurred while creating the notification: {ex.Message}" });
        }
    }
    
    // POST: api/notification/mark-as-seen
    [HttpPost("mark-as-seen")]
    public async Task<IActionResult> MarkNotificationsAsSeen([FromBody] MarkNotificationsAsSeenDto request)
    {
        try
        {
            await _notificationService.MarkNotificationsAsSeenAsync(request.BusinessId, request.NotificationIds);
            return Ok(new { message = "Notifications marked as seen successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while marking notifications: {ex.Message}" });
        }
    }
}