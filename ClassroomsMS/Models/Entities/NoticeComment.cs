namespace ClassroomsMS.Models.Entities;

public class NoticeComment
{
    public int Id { get; set; }
    public int NoticeId { get; set; }
    public int UserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Notice Notice { get; set; } = null!;
    public User User { get; set; } = null!;
}
