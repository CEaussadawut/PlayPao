namespace PlayPao.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string? User { get; set; } // Recipient
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string? Type { get; set; } // "join_request", "request_approved", "request_rejected"
        public int? EventId { get; set; } // Related event
    }
}