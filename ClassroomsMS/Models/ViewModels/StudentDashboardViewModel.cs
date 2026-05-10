using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Models.ViewModels;

public class StudentDashboardViewModel
{
    public List<Classroom> Classrooms { get; set; } = new();
    public int StudentUserId { get; set; }
}
