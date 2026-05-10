using ClassroomsMS.Models.Entities;

namespace ClassroomsMS.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Users.Any()) return;

        // Seed classes
        var classes = new[]
        {
            new Class { Grade = 5, ClassNumber = 1 },
            new Class { Grade = 5, ClassNumber = 2 },
            new Class { Grade = 6, ClassNumber = 1 },
            new Class { Grade = 6, ClassNumber = 2 },
        };
        context.Classes.AddRange(classes);
        context.SaveChanges();

        // Seed teacher users
        var teacherUsers = new[]
        {
            new User { FirstName = "Ana", LastName = "Markovic", Email = "ana@school.com", Password = "teacher123", Role = UserRole.Teacher },
            new User { FirstName = "Ivan", LastName = "Petrovic", Email = "ivan@school.com", Password = "teacher123", Role = UserRole.Teacher },
        };
        context.Users.AddRange(teacherUsers);
        context.SaveChanges();

        var teacherProfiles = teacherUsers.Select(t => new TeacherUser { UserId = t.Id }).ToArray();
        context.TeacherUsers.AddRange(teacherProfiles);
        context.SaveChanges();
    }
}
