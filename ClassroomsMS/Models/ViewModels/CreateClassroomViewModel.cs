using ClassroomsMS.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ClassroomsMS.Models.ViewModels;

public class CreateClassroomViewModel
{
    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Class")]
    public int ClassId { get; set; }

    public List<Class> Classes { get; set; } = new();
}
