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
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
