namespace ClassroomsMS.Models.Entities;

public class Notice
{
    public int Id { get; set; }
    public int ClassroomId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Classroom Classroom { get; set; } = null!;
    public ICollection<NoticeImage> Images { get; set; } = new List<NoticeImage>();
    public ICollection<NoticeComment> Comments { get; set; } = new List<NoticeComment>();
}
