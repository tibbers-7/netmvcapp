namespace ClassroomsMS.Models.Entities;

public class TeacherUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}
