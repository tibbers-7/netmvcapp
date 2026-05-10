namespace ClassroomsMS.Models.Entities;

public class TaskFile
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public int StudentUserId { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Assignment Assignment { get; set; } = null!;
    public StudentUser StudentUser { get; set; } = null!;
}
