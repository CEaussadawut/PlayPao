namespace PlayPao.Models
{
    public class UserProfile
    {
        public string UserName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string AvatarUrl { get; set; } = "/images/profile.png";
    }
}
