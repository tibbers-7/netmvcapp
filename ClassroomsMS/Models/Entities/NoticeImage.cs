namespace ClassroomsMS.Models.Entities;

public class NoticeImage
{
    public int Id { get; set; }
    public int NoticeId { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public Notice Notice { get; set; } = null!;
}
