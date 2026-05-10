using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Models.ViewModels;

public class ClassroomTasksViewModel
{
    public Classroom Classroom { get; set; } = null!;
    public bool IsTeacher { get; set; }
    public int? StudentUserId { get; set; }
    public int TotalStudents { get; set; }
}
