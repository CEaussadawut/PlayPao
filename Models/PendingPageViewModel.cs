using System.Collections.Generic;

namespace PlayPao.Models
{
    public class PendingPageViewModel
    {
        public List<PendingRequestViewModel> MyPendingRequests { get; set; } = new List<PendingRequestViewModel>();
        public List<EventPendingRequestsViewModel> EventPendingRequests { get; set; } = new List<EventPendingRequestsViewModel>();
        public string ActiveTab { get; set; } = "my"; // "my" or "manage"
    }

    public class EventPendingRequestsViewModel
    {
        public Event Event { get; set; } = null!;
        public List<EventJoinRequest> PendingRequests { get; set; } = new List<EventJoinRequest>();
    }
}