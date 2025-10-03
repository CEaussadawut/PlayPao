namespace PlayPao.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; } // For storing uploaded image path
        public string Description { get; set; }
        public int Member { get; set; } // Max number of members
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Location { get; set; }
        public int CurrentMembers { get; set; } = 0; // Track current joined members
    }
}