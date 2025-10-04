namespace PlayPao.Models
{
    public class EventJoinRequest
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string? User { get; set; } // Username of the requester
        public string? Status { get; set; } // "Pending", "Approved", "Rejected"
        public DateTime RequestedAt { get; set; }
    }
}