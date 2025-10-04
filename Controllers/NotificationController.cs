using PlayPao.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PlayPao.Controllers;

public class NotificationController : Controller
{
    private readonly ILogger<AuthController> _logger;

    public NotificationController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var notifications = EventController.GetNotifications()
            .Where(n => n.User == currentUser)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return View(notifications);
    }

    [HttpPost]
    public IActionResult MarkAsRead(int id)
    {
        var notification = EventController.GetNotifications().FirstOrDefault(n => n.Id == id);
        if (notification != null)
        {
            notification.IsRead = true;
        }
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult GetUnreadCount()
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return Json(0);
        }

        var count = EventController.GetNotifications()
            .Count(n => n.User == currentUser && !n.IsRead);

        return Json(count);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
