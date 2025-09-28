using PlayPao.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PlayPao.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Bookmark()
    {
        return View();
    }

    public IActionResult Ticket()
    {
        return View();
    }

    public IActionResult AboutUs()
    {
        return View();
    }

    public IActionResult Profile()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("User")))
            return RedirectToAction("Login", "Auth");

        ViewBag.Name = HttpContext.Session.GetString("User");
        ViewBag.Phone = "123-456-7890";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
