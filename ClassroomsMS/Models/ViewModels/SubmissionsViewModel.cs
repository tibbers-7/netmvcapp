using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Models.ViewModels;

public class SubmissionsViewModel
{
    public int AssignmentId { get; set; }
    public List<TaskFile> Submissions { get; set; } = new();
    public int TotalStudents { get; set; }
}
