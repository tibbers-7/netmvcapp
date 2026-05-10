using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Models.ViewModels;

public class ClassroomNoticesViewModel
{
    public Classroom Classroom { get; set; } = null!;
    public bool IsTeacher { get; set; }
    public int CurrentUserId { get; set; }
}
