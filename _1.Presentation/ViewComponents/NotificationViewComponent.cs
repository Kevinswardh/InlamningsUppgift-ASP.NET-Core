using ApplicationLayer_ServiceLayer_.NotificationManagment.NotificationService.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.ViewComponents
{
    public class NotificationCardViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;

        public NotificationCardViewComponent(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return View(new List<__Cross_cutting_Concerns.FormDTOs.NotificationForm>());

            var notifications = await _notificationService.GetUnreadByUserAsync(userId);
            return View("~/Views/Shared/_NotificationCard.cshtml", notifications);
        }
    }
}
