namespace ClassroomsMS.Models.Entities;

public class Class
{
    public int Id { get; set; }
    public int Grade { get; set; }
    public int ClassNumber { get; set; }

    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    public ICollection<StudentUser> Students { get; set; } = new List<StudentUser>();
}
