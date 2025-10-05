using Microsoft.AspNetCore.Mvc;
using PlayPao.Models;
using PlayPao.Controllers;
using System.Linq;

namespace PlayPao.ViewComponents
{
    public class UpcomingEventsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var currentUser = HttpContext.Session.GetString("User");
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                return View(Enumerable.Empty<Event>());
            }

            // Get events that the user has joined and haven't started yet
            var upcomingEvents = EventController.GetEvents()
                .Where(e => e.JoinedUsers.Contains(currentUser))
                .Where(e => DateTime.Now < e.Date + e.Time)
                .OrderBy(e => e.Date + e.Time)
                .Take(3)
                .ToList();

            return View(upcomingEvents);
        }
    }
}