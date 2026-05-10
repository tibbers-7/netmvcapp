namespace ClassroomsMS.Models.Entities;

public class ClassroomStudent
{
    public int Id { get; set; }
    public int ClassroomId { get; set; }
    public int StudentUserId { get; set; }

    public Classroom Classroom { get; set; } = null!;
    public StudentUser StudentUser { get; set; } = null!;
}
