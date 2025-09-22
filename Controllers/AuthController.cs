using PlayPao.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PlayPao.Controllers;

// [Authorize]
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;

    private static readonly Dictionary<string, string> Users = new();

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password, bool remember = false)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Username and password are required.";
            return View();
        }
        if (Users.TryGetValue(username, out var storedPassword) && storedPassword == password)
        {
            HttpContext.Session.SetString("User", username);
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Error = "Invalid username or password.";
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string password, string confirm_password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm_password))
        {
            ViewBag.Error = "All fields are required.";
            return View();
        }
        if (password != confirm_password)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }
        if (Users.ContainsKey(username))
        {
            ViewBag.Error = "Username already exists.";
            return View();
        }
        Users[username] = password;
        HttpContext.Session.SetString("User", username);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("User");
        return RedirectToAction("Login");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
