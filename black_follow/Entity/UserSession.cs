using black_follow.Entity;

public class UserSession
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }  // Unique session token
    public DateTime LoginTime { get; set; }
    public string Device { get; set; } // Could be browser or device information
    public string IPAddress { get; set; } // Store IP address of the user
    public bool IsActive { get; set; }  // To track active/inactive sessions

    public virtual AppUser User { get; set; }
}