using TakiWebApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetAllNotifications()
    {
        var notifications = await _notificationRepository.GetAllNotificationsAsync();
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(int id)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(id);
        
        if (notification == null)
        {
            return NotFound();
        }
        if (!User.IsPartyOf(notification.UserID, notification.DriverID))
        {
            return Forbid();
        }

        return Ok(notification);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByUserId(int userId)
    {
        var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("user/{userId}/unread")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetUnreadNotificationsByUserId(int userId)
    {
        var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        var unreadNotifications = notifications.Where(n => !n.IsRead);
        return Ok(unreadNotifications);
    }

    [HttpGet("user/{userId}/unread/count")]
    public async Task<ActionResult<int>> GetUnreadNotificationsCountByUserId(int userId)
    {
        // Yalnızca bu kullanıcının okunmamış bildirimleri (önceden tüm sistem sayılıyordu).
        var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        return Ok(notifications.Count(n => !n.IsRead));
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetActiveNotifications()
    {
        var notifications = await _notificationRepository.GetAllNotificationsAsync();
        return Ok(notifications);
    }

    [HttpGet("paginated")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var notifications = await _notificationRepository.GetNotificationsPaginatedAsync(pageNumber, pageSize);
        return Ok(notifications);
    }

    [HttpGet("count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetTotalNotificationsCount()
    {
        var count = await _notificationRepository.GetTotalNotificationsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetActiveNotificationsCount()
    {
        var count = await _notificationRepository.GetTotalNotificationsCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Notification>>> SearchNotificationsByTitle([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty.");
        }

        var notifications = await _notificationRepository.SearchNotificationsByTitleAsync(searchTerm);
        return Ok(notifications);
    }

    [HttpGet("date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date.");
        }

        var notifications = await _notificationRepository.GetNotificationsByCreatedDateRangeAsync(startDate, endDate);
        return Ok(notifications);
    }

    [HttpPut("{id}/mark-as-read")]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        try
        {
            var existingNotification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (existingNotification == null)
            {
                return NotFound();
            }
            if (!User.IsPartyOf(existingNotification.UserID, existingNotification.DriverID))
            {
                return Forbid();
            }

            var success = await _notificationRepository.MarkAsReadAsync(id);
            
            if (!success)
            {
                return StatusCode(500, "Failed to mark notification as read.");
            }

            return NoContent();
        }
        catch
        {
            throw;
        }
    }

    [HttpPut("user/{userId}/mark-all-as-read")]
    public async Task<IActionResult> MarkAllNotificationsAsReadByUserId(int userId)
    {
        try
        {
            // Get all unread notifications for user and mark them as read
            var unreadNotifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
            var success = true;
            
            foreach (var notification in unreadNotifications.Where(n => !n.IsRead))
            {
                var result = await _notificationRepository.MarkAsReadAsync(notification.NotificationID);
                if (!result) success = false;
            }
            
            if (!success)
            {   
                return StatusCode(500, "Failed to mark all notifications as read.");
            }

            return NoContent();
        }
        catch
        {
            throw;
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Notification>> CreateNotification([FromBody] Notification notification)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var notificationId = await _notificationRepository.CreateNotificationAsync(notification);
            var createdNotification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            
            return CreatedAtAction(nameof(GetNotification), new { id = notificationId }, createdNotification);
        }
        catch
        {
            throw;
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNotification(int id, [FromBody] Notification notification)
    {
        if (id != notification.NotificationID)
        {
            return BadRequest("ID mismatch between route and body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingNotification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (existingNotification == null)
            {
                return NotFound();
            }

            notification.UpdatedDate = DateTime.UtcNow;
            var success = await _notificationRepository.UpdateNotificationAsync(notification);
            
            if (!success)
            {
                return StatusCode(500, "Failed to update notification.");
            }

            return NoContent();
        }
        catch
        {
            throw;
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        try
        {
            var existingNotification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (existingNotification == null)
            {
                return NotFound();
            }

            var success = await _notificationRepository.DeleteNotificationAsync(id);
            
            if (!success)
            {
                return StatusCode(500, "Failed to delete notification.");
            }

            return NoContent();
        }
        catch
        {
            throw;
        }
    }
}
