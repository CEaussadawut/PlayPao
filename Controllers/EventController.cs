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
    private static List<EventJoinRequest> _joinRequests = new List<EventJoinRequest>();
    private static int _nextJoinRequestId = 1;
    private static List<Notification> _notifications = new List<Notification>();
    private static int _nextNotificationId = 1;
    private static List<ChatMessage> _chatMessages = new List<ChatMessage>();
    private static int _nextChatMessageId = 1;

    // Keep Ticket Per User
    public static readonly Dictionary<string, List<Ticket>> TicketsByUser = new();

    private readonly ILogger<EventController> _logger;

    public EventController(ILogger<EventController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult RequestJoin(int eventId)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound();
        }

        // Check if event has already started
        var eventStartTime = eventItem.Date + eventItem.Time;
        if (DateTime.Now >= eventStartTime)
        {
            TempData["Message"] = "This event has already started or passed.";
            return RedirectToAction("Detail", new { id = eventId });
        }

        // Check if already requested or joined
        if (_joinRequests.Any(r => r.EventId == eventId && r.User == currentUser) ||
            eventItem.JoinedUsers.Contains(currentUser))
        {
            TempData["Message"] = "You have already requested to join or are already a member.";
            return RedirectToAction("Detail", new { id = eventId });
        }

        var request = new EventJoinRequest
        {
            Id = _nextJoinRequestId++,
            EventId = eventId,
            User = currentUser,
            Status = "Pending",
            RequestedAt = DateTime.Now
        };

        _joinRequests.Add(request);

        // Create notification for event creator
        var notification = new Notification
        {
            Id = _nextNotificationId++,
            User = eventItem.Creator,
            Message = $"{currentUser} requested to join your event '{eventItem.Title}'.",
            Type = "join_request",
            EventId = eventId
        };
        _notifications.Add(notification);

        TempData["Message"] = "Join request sent. Waiting for approval.";
        return RedirectToAction("Detail", new { id = eventId });
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
        ViewBag.JoinRequests = _joinRequests;
        ViewBag.ChatMessages = _chatMessages.Where(m => m.EventId == id).OrderBy(m => m.Timestamp).ToList();
        var joinedUsernames = new HashSet<string>(eventItem.JoinedUsers);
        joinedUsernames.Add(eventItem.Creator!);
        var profiles = joinedUsernames
            .Select(u => AuthController.Profiles.TryGetValue(u, out var p) ? p : null)
            .Where(p => p != null)
            .ToList();
        // Sort so creator is first
        profiles = profiles.OrderByDescending(p => p.UserName == eventItem.Creator!).ToList();
        ViewBag.JoinedUserProfiles = profiles;
        return View(eventItem);
    }

    public IActionResult PendingRequests(int eventId)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventItem == null || eventItem.Creator != currentUser)
        {
            return Forbid();
        }

        var pendingRequests = _joinRequests.Where(r => r.EventId == eventId && r.Status == "Pending").ToList();
        ViewBag.Event = eventItem;
        return View(pendingRequests);
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
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

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

        foreach (var kv in TicketsByUser)
        {
            kv.Value.RemoveAll(t => t.EventId == id);
        }

        return RedirectToAction("MyEvents");
    }

    // helper
    private static Event? FindEventById(int id) => _events.FirstOrDefault(e => e.Id == id);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Join(int id)
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user))
            return RedirectToAction("Login", "Auth");

        var ev = FindEventById(id);
        if (ev == null) return NotFound();

        // Check if event has already started
        var eventStartTime = ev.Date + ev.Time;
        if (DateTime.Now >= eventStartTime)
        {
            TempData["Info"] = "This event has already started.";
            return RedirectToAction("Detail", new { id });
        }

        if (ev.CurrentMembers >= ev.Member)
        {
            TempData["Info"] = "Event is full.";
            return RedirectToAction("Detail", new { id });
        }

        if (!TicketsByUser.TryGetValue(user, out var list))
        {
            list = new List<Ticket>();
            TicketsByUser[user] = list;
        }

        if (list.Any(t => t.EventId == id))
        {
            TempData["toast"] = "You already joined this event.";
            return RedirectToAction("Ticket", "Home");
        }

        var fallbackImg = "https://i.ytimg.com/vi/N_Fb4x-OAzE/oardefault.jpg";

        // Build Ticket 
        list.Add(new Ticket
        {
            Id = list.Count == 0 ? 1 : list.Max(t => t.Id) + 1,
            EventId = ev.Id,
            EventTitle = ev.Title ?? string.Empty,
            Description = ev.Description ?? string.Empty,
            Creator = ev.Creator ?? string.Empty,
            ImagePath = string.IsNullOrWhiteSpace(ev.ImagePath) ? fallbackImg : ev.ImagePath!,
            Location = ev.Location ?? string.Empty,
            Date = ev.Date,
            Time = ev.Time,
            EndTime = ev.EndTime
        });

        ev.CurrentMembers = Math.Min(ev.CurrentMembers + 1, ev.Member);

        TempData["toast"] = "Joined successfully!";
        return RedirectToAction("Ticket", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LeaveEvent(int eventId)
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user))
            return RedirectToAction("Login", "Auth");

        var ev = FindEventById(eventId);
        if (ev == null) return NotFound();

        if (!ev.JoinedUsers.Contains(user))
        {
            TempData["toast"] = "You are not a member of this event.";
            return RedirectToAction("Ticket", "Home");
        }

        // Remove user from joined users
        ev.JoinedUsers.Remove(user);
        ev.CurrentMembers = Math.Max(ev.CurrentMembers - 1, 0);

        // Remove ticket
        if (TicketsByUser.TryGetValue(user, out var list))
        {
            list.RemoveAll(t => t.EventId == eventId);
        }

        // Remove any existing join requests for this user and event
        _joinRequests.RemoveAll(r => r.EventId == eventId && r.User == user);

        // Create notification for event creator
        var notification = new Notification
        {
            Id = _nextNotificationId++,
            User = ev.Creator,
            Message = $"{user} has cancelled their registration for your event '{ev.Title}'.",
            Type = "member_cancelled",
            EventId = eventId
        };
        _notifications.Add(notification);

        TempData["toast"] = "Registration cancelled successfully!";
        return RedirectToAction("Ticket", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SendMessage(int eventId, string message)
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrWhiteSpace(user))
        {
            return RedirectToAction("Login", "Auth");
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound();
        }

        // Check if user can chat (creator or joined)
        if (eventItem.Creator != user && !eventItem.JoinedUsers.Contains(user))
        {
            TempData["Message"] = "You must be a member of this event to chat.";
            return RedirectToAction("Detail", new { id = eventId });
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return RedirectToAction("Detail", new { id = eventId });
        }

        var chatMessage = new ChatMessage
        {
            Id = _nextChatMessageId++,
            EventId = eventId,
            User = user,
            Message = message,
            Timestamp = DateTime.Now
        };

        _chatMessages.Add(chatMessage);

        return RedirectToAction("Detail", new { id = eventId });
    }

    public static Event? FindEvent(int id) => _events.FirstOrDefault(e => e.Id == id);

    [HttpPost]
    public IActionResult ApproveJoin(int requestId)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var request = _joinRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return NotFound();
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == request.EventId);
        if (eventItem == null || eventItem.Creator != currentUser)
        {
            return Forbid();
        }

        // Check if event has already started
        var eventStartTime = eventItem.Date + eventItem.Time;
        if (DateTime.Now >= eventStartTime)
        {
            TempData["Error"] = "This event has already started.";
            return RedirectToAction("Pending", new { tab = "manage" });
        }

        if (eventItem.CurrentMembers >= eventItem.Member)
        {
            TempData["Error"] = "Event is full.";
            return RedirectToAction("Pending", new { tab = "manage" });
        }

        request.Status = "Approved";
        if (request.User != null)
        {
            eventItem.JoinedUsers.Add(request.User);
            eventItem.CurrentMembers++;

            // Create ticket for the approved user
            var fallbackImg = "https://i.ytimg.com/vi/N_Fb4x-OAzE/oardefault.jpg";
            if (!TicketsByUser.TryGetValue(request.User, out var list))
            {
                list = new List<Ticket>();
                TicketsByUser[request.User] = list;
            }
            if (!list.Any(t => t.EventId == request.EventId))
            {
                list.Add(new Ticket
                {
                    Id = list.Count == 0 ? 1 : list.Max(t => t.Id) + 1,
                    EventId = eventItem.Id,
                    EventTitle = eventItem.Title ?? string.Empty,
                    Description = eventItem.Description ?? string.Empty,
                    Creator = eventItem.Creator ?? string.Empty,
                    ImagePath = string.IsNullOrWhiteSpace(eventItem.ImagePath) ? fallbackImg : eventItem.ImagePath,
                    Location = eventItem.Location ?? string.Empty,
                    Date = eventItem.Date,
                    Time = eventItem.Time,
                    EndTime = eventItem.EndTime
                });
            }

            // Create notification for the approved user
            var notification = new Notification
            {
                Id = _nextNotificationId++,
                User = request.User,
                Message = $"Your join request for '{eventItem.Title}' has been approved!",
                Type = "request_approved",
                EventId = request.EventId
            };
            _notifications.Add(notification);
        }
        TempData["Message"] = "Request approved.";
        return RedirectToAction("Pending", new { tab = "manage" });
    }

    [HttpPost]
    public IActionResult RejectJoin(int requestId)
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var request = _joinRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return NotFound();
        }

        var eventItem = _events.FirstOrDefault(e => e.Id == request.EventId);
        if (eventItem == null || eventItem.Creator != currentUser)
        {
            return Forbid();
        }

        request.Status = "Rejected";

        // Create notification for the rejected user
        var notification = new Notification
        {
            Id = _nextNotificationId++,
            User = request.User,
            Message = $"Your join request for '{eventItem.Title}' has been rejected.",
            Type = "request_rejected",
            EventId = request.EventId
        };
        _notifications.Add(notification);

        TempData["Message"] = "Request rejected.";
        return RedirectToAction("Pending", new { tab = "manage" });
    }


    public IActionResult MyPendingRequests()
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var pendingRequests = _joinRequests.Where(r => r.User == currentUser && r.Status == "Pending")
                                           .Select(r => new PendingRequestViewModel
                                           {
                                               Request = r,
                                               Event = _events.FirstOrDefault(e => e.Id == r.EventId) ?? new Event()
                                           })
                                           .ToList();
        return View(pendingRequests);
    }

    public IActionResult Pending(string tab = "my")
    {
        var currentUser = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(currentUser))
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = new PendingPageViewModel { ActiveTab = tab };

        // My Pending Requests
        viewModel.MyPendingRequests = _joinRequests
            .Where(r => r.User == currentUser && r.Status == "Pending")
            .Select(r => new PendingRequestViewModel
            {
                Request = r,
                Event = _events.FirstOrDefault(e => e.Id == r.EventId) ?? new Event()
            })
            .ToList();

        // Event Pending Requests (สำหรับกิจกรรมที่คุณสร้าง)
        var myEvents = _events.Where(e => e.Creator == currentUser);
        foreach (var eventItem in myEvents)
        {
            var pendingRequests = _joinRequests
                .Where(r => r.EventId == eventItem.Id && r.Status == "Pending")
                .ToList();

            if (pendingRequests.Any())
            {
                viewModel.EventPendingRequests.Add(new EventPendingRequestsViewModel
                {
                    Event = eventItem,
                    PendingRequests = pendingRequests
                });
            }
        }

        return View(viewModel);
    }

    // Method to get events for Index
    public static List<Event> GetEvents()
    {
        return _events.OrderByDescending(e => e.Date).ToList();
    }

    // Method to get join requests
    public static List<EventJoinRequest> GetJoinRequests()
    {
        return _joinRequests;
    }

    // Method to get notifications
    public static List<Notification> GetNotifications()
    {
        return _notifications;
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
