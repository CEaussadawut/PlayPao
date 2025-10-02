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
        return View();
    }

    [HttpPost]
    public IActionResult CreateEvent(string title, IFormFile image, string description, string member, DateTime date, string time, string endtime, string location)
    {
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
            Location = location
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