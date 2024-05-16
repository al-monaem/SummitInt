using BLL.Manager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Summit_Interview.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationManager _notificationManager;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationController(NotificationManager notificationManager, UserManager<IdentityUser> userManager)
        {
            _notificationManager = notificationManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetFileUploadsNotification()
        {
            var userid = _userManager.GetUserId(User);
            var notifications = await _notificationManager.GetFileUploadsNotifications(userid);

            return Json(new
            {
                Status = 200,
                Data = notifications
            });
        }
    }
}
