namespace ClassroomsMS.Models.Entities;

public class Classroom
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string Code { get; set; } = string.Empty;

    public Class Class { get; set; } = null!;
    public TeacherUser Teacher { get; set; } = null!;
    public ICollection<Notice> Notices { get; set; } = new List<Notice>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();
}
