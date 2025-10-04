using System.Collections.Generic;

namespace PlayPao.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ImagePath { get; set; }
        public string? Description { get; set; }
        public int Member { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public int CurrentMembers { get; set; } = 0;
        public string? Creator { get; set; }
        public List<string> JoinedUsers { get; set; } = new List<string>();
    }
}