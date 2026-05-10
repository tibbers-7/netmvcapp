namespace ClassroomsMS.Models.Entities;

public class TaskAttachment
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public Assignment Assignment { get; set; } = null!;
}
