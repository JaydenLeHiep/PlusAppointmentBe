using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.NotificationService;

namespace PlusAppointment.Controllers.NotificationController;

[ApiController]
[Route("api/[controller]")]
public class NotificationController: ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: api/notification/{businessId}
    // GET: api/notification/business_id{businessId}/get-notifications
    [Authorize] 
    [HttpGet("business_id={businessId}/get-notifications")]
    public async Task<IActionResult> GetNotifications(int businessId)
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString() && userRole != Role.Owner.ToString())
        {
            return NotFound(new { message = "You are not authorized to update this staff." });
        }
        
        try
        {
            var notifications = await _notificationService.GetNotificationsByBusinessIdAsync(businessId);

            if (notifications == null || !notifications.Any())
            {
                return NotFound(new { message = "No notifications found for this business." });
            }

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while retrieving notifications: {ex.Message}" });
        }
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
            await _notificationService.AddNotificationAsync(request.BusinessId, request.Message, request.NotificationType);
            return Ok(new { message = "Notification created successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while creating the notification: {ex.Message}" });
        }
    }
}