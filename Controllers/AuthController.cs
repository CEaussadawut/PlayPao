using PlayPao.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PlayPao.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;

    // Keep username/password in memory
    private static readonly Dictionary<string, string> Users = new();

    //One profile per user use with HomeController
    public static readonly Dictionary<string, UserProfile> Profiles = new();

    private const string DefaultAvatar = "/images/profile.png";

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    // ---------- Login ----------
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
            // Create profile if dont have
            var profile = GetOrCreateProfile(username);

            // Reset session other user
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("User", username);
            HttpContext.Session.SetString("AvatarUrl", profile.AvatarUrl);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Invalid username or password.";
        return View();
    }

    // ---------- Register ----------
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string password, string confirm_password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirm_password))
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

        // Add new user + IntitialProfile
        Users[username] = password;
        var profile = GetOrCreateProfile(username);

        HttpContext.Session.Clear();
        HttpContext.Session.SetString("User", username);
        HttpContext.Session.SetString("AvatarUrl", profile.AvatarUrl);

        return RedirectToAction("Index", "Home");
    }

    // ---------- Logout ----------
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete(".AspNetCore.Session");
        return RedirectToAction("Login");
    }

    // ---------- Error ----------
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // ---------- Helpers ----------
    private static UserProfile GetOrCreateProfile(string username)
    {
        if (!Profiles.TryGetValue(username, out var p))
        {
            p = new UserProfile
            {
                UserName = username,
                DisplayName = username,
                Phone = "",
                Email = "",
                AvatarUrl = DefaultAvatar
            };
            Profiles[username] = p;
        }
        return p;
    }
}
