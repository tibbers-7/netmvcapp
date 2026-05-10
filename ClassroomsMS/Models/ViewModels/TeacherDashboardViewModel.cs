using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Models.ViewModels;

public class TeacherDashboardViewModel
{
    public List<Classroom> Classrooms { get; set; } = new();
}
