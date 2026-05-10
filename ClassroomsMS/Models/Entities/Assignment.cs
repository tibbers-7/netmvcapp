namespace ClassroomsMS.Models.Entities;

public class Assignment
{
    public int Id { get; set; }
    public int ClassroomId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Classroom Classroom { get; set; } = null!;
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    public ICollection<TaskFile> TaskFiles { get; set; } = new List<TaskFile>();
}
