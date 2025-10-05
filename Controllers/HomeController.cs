using PlayPao.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace PlayPao.Controllers;

public class HomeController : Controller
{
    public static readonly Dictionary<string, List<int>> BookmarksByUser = new();

    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(string search)
    {
        var events = EventController.GetEvents();
        events = events.Where(e => DateTime.Now < e.Date + e.Time).ToList();
        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events.Where(e => e.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       e.Description.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        ViewBag.JoinRequests = EventController.GetJoinRequests();
        ViewBag.Search = search;
        return View(events);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Bookmark()
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user)) return RedirectToAction("Login", "Auth");

        BookmarksByUser.TryGetValue(user, out var eventIds);
        var events = eventIds?.Select(id => EventController.FindEvent(id)).Where(e => e != null).ToList() ?? new List<Event>();
        ViewBag.JoinRequests = EventController.GetJoinRequests();
        return View(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleBookmark(int eventId)
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user)) return RedirectToAction("Login", "Auth");

        if (!BookmarksByUser.TryGetValue(user, out var list))
        {
            list = new List<int>();
            BookmarksByUser[user] = list;
        }

        if (list.Contains(eventId))
        {
            list.Remove(eventId);
        }
        else
        {
            list.Add(eventId);
        }

        return RedirectToAction("Index");
    }

    public IActionResult Ticket()
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user))
            return RedirectToAction("Login", "Auth");

        EventController.TicketsByUser.TryGetValue(user, out var tickets);
        return View(tickets ?? Enumerable.Empty<Ticket>());
    }

    public IActionResult AboutUs()
    {
        return View();
    }

    public IActionResult Profile()
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user)) return RedirectToAction("Login", "Auth");

        var p = AuthController.Profiles.TryGetValue(user, out var prof)
            ? prof
            : new UserProfile { UserName = user, DisplayName = user };

        ViewBag.Name = p.DisplayName;
        ViewBag.Phone = p.Phone;
        ViewBag.Email = p.Email;
        ViewBag.AvatarUrl = p.AvatarUrl;
        return View();
    }

    [HttpGet]
    public IActionResult EditProfile()
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user)) return RedirectToAction("Login", "Auth");

        if (!AuthController.Profiles.TryGetValue(user, out var p))
            p = new UserProfile { UserName = user, DisplayName = user };

        ViewBag.Name = p.DisplayName;
        ViewBag.Phone = p.Phone;
        ViewBag.Email = p.Email;
        ViewBag.AvatarUrl = p.AvatarUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditProfile(string name, string phone, string avatarUrl, string email)
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user)) return RedirectToAction("Login", "Auth");

        if (string.IsNullOrWhiteSpace(name))
        {
            ViewBag.Error = "Name is required.";
            ViewBag.Name = name; ViewBag.Phone = phone; ViewBag.Email = email;
            ViewBag.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? "/images/profile.png" : avatarUrl;
            return View();
        }

        if (!AuthController.Profiles.TryGetValue(user, out var p))
            p = AuthController.Profiles[user] = new UserProfile { UserName = user, DisplayName = user };

        p.DisplayName = name;
        p.Phone = phone ?? "";
        p.Email = email ?? "";
        p.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? "/images/profile.png" : avatarUrl;

        HttpContext.Session.SetString("AvatarUrl", p.AvatarUrl);

        return RedirectToAction(nameof(Profile));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
