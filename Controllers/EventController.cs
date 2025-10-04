using PlayPao.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace PlayPao.Controllers;

public class EventController : Controller
{
    // Static list to store events (in production, use a database)
    private static List<Event> _events = new List<Event>();
    private static int _nextId = 1;

    private readonly ILogger<EventController> _logger;

    public EventController(ILogger<EventController> logger)
    {
        _logger = logger;
    }

    public IActionResult CreateEvent()
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }
        return View();
    }

    [HttpPost]
    public IActionResult CreateEvent(string title, IFormFile image, string description, string member, DateTime date, string time, string endtime, string location)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            ViewBag.Error = "Title and description are required.";
            return View();
        }

        if (!int.TryParse(member, out int memberCount) || memberCount <= 0)
        {
            ViewBag.Error = "Member count must be a positive number.";
            return View();
        }

        if (!TimeSpan.TryParse(time, out TimeSpan startTime))
        {
            ViewBag.Error = "Invalid start time format.";
            return View();
        }

        if (!TimeSpan.TryParse(endtime, out TimeSpan endTime))
        {
            ViewBag.Error = "Invalid end time format.";
            return View();
        }

        var newEvent = new Event
        {
            Id = _nextId++,
            Title = title,
            Description = description,
            Member = memberCount,
            Date = date,
            Time = startTime,
            EndTime = endTime,
            Location = location,
            Creator = currentUser

        };

        // Handle image upload
        if (image != null && image.Length > 0)
        {
            var fileName = $"{newEvent.Id}_{image.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            newEvent.ImagePath = $"/images/events/{fileName}";
        }

        _events.Add(newEvent);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Detail(int id)
    {
        var eventItem = _events.FirstOrDefault(e => e.Id == id);
        if (eventItem == null)
        {
            return NotFound();
        }
        return View(eventItem);
    }

    public IActionResult MyEvents()
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var myEvents = _events.Where(e => e.Creator == currentUser).OrderByDescending(e => e.Date).ToList();
        return View(myEvents);
    }

    public IActionResult Edit(int id)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == id);
        if (eventItem == null)
        {
            return NotFound();
        }

        if (eventItem.Creator != currentUser)
        {
            return Forbid();
        }

        return View(eventItem);
    }

    [HttpPost]
    public IActionResult Edit(int id, string title, IFormFile image, string description, string member, DateTime date, string time, string endtime, string location)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == id);
        if (eventItem == null)
        {
            return NotFound();
        }

        if (eventItem.Creator != currentUser)
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            ViewBag.Error = "Title and description are required.";
            return View(eventItem);
        }

        if (!int.TryParse(member, out int memberCount) || memberCount <= 0)
        {
            ViewBag.Error = "Member count must be a positive number.";
            return View(eventItem);
        }

        if (!TimeSpan.TryParse(time, out TimeSpan startTime))
        {
            ViewBag.Error = "Invalid start time format.";
            return View(eventItem);
        }

        if (!TimeSpan.TryParse(endtime, out TimeSpan endTime))
        {
            ViewBag.Error = "Invalid end time format.";
            return View(eventItem);
        }

        eventItem.Title = title;
        eventItem.Description = description;
        eventItem.Member = memberCount;
        eventItem.Date = date;
        eventItem.Time = startTime;
        eventItem.EndTime = endTime;
        eventItem.Location = location;

        // Handle image upload if provided
        if (image != null && image.Length > 0)
        {
            var fileName = $"{eventItem.Id}_{image.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            eventItem.ImagePath = $"/images/events/{fileName}";
        }

        return RedirectToAction("MyEvents");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == id);
        if (eventItem == null)
        {
            return NotFound();
        }

        if (eventItem.Creator != currentUser)
        {
            return Forbid();
        }

        _events.Remove(eventItem);
        return RedirectToAction("MyEvents");
    }


    // Method to get events for Index
    public static List<Event> GetEvents()
    {
        return _events.OrderByDescending(e => e.Date).ToList();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult DetailMockup()
    {
        return View("Detail");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}