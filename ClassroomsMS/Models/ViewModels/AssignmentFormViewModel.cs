using ClassroomsMS.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ClassroomsMS.Models.ViewModels;

public class AssignmentFormViewModel
{
    public int? Id { get; set; }
    public int ClassroomId { get; set; }
    public string ClassroomName { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;

    public List<TaskAttachment> ExistingAttachments { get; set; } = new();
}
