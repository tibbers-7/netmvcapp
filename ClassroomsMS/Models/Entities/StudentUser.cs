namespace ClassroomsMS.Models.Entities;

public class StudentUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ClassId { get; set; }

    public User User { get; set; } = null!;
    public Class? Class { get; set; }
    public ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();
    public ICollection<TaskFile> TaskFiles { get; set; } = new List<TaskFile>();
}
