namespace ClassroomsMS.Models.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public TeacherUser? TeacherUser { get; set; }
    public StudentUser? StudentUser { get; set; }
}
