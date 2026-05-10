using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Models.ViewModels;

public class ClassroomStudentsViewModel
{
    public Classroom Classroom { get; set; } = null!;
    public List<Class> AllClasses { get; set; } = new();
    public List<StudentUser> AvailableStudents { get; set; } = new();
}
