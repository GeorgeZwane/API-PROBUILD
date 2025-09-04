using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Models;

namespace ProBuild_API.Controllers
{
    [Route("api/webtask")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly ProBuildDbContext dbContext;
        public NotificationController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("sendmessage")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDTO dto)
        {
          
            var notification = new Notification
            {
                SenderId = dto.SenderId,
                RecipientId = dto.RecipientId,
                Message = dto.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            return Ok("Notification sent.");
        }

        [HttpGet("getMessagesforforeman/{foremanId}")]
        public IActionResult GetNotificationsForForeman(int foremanId)
        {
            var notifications = dbContext.Notifications
                .Where(n => n.RecipientId == foremanId)
                .OrderByDescending(n => n.SentAt)
                .ToList();

            return Ok(notifications);
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await dbContext.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await dbContext.SaveChangesAsync();
            return Ok();
        }

    }
}
