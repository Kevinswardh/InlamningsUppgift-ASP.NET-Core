using Microsoft.AspNetCore.Mvc;
using ApplicationLayer_ServiceLayer_.NotificationManagment.NotificationService.Interface;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;

namespace _1.PresentationLayer.Controllers
{
    [Route("notifications")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService; // <-- lägg till detta

        public NotificationsController(
            INotificationService notificationService,
            IUserService userService) // <-- injicera här
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpGet("getunreadcount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userService.GetUserId(User);
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }
    }
}
