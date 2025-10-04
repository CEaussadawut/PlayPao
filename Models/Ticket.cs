namespace PlayPao.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";
        public string Description { get; set; } = "";
        public string Creator { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
