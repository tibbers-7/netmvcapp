using ClassroomsMS.Data;
using ClassroomsMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Controllers;

public class DashboardController : BaseController
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Teacher()
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var teacherUser = await _db.TeacherUsers
            .Include(t => t.Classrooms)
                .ThenInclude(c => c.Class)
            .FirstOrDefaultAsync(t => t.UserId == CurrentUserId);

        if (teacherUser == null) return RedirectToAction("Index", "Account");

        return View(new TeacherDashboardViewModel
        {
            Classrooms = teacherUser.Classrooms.ToList()
        });
    }

    public async Task<IActionResult> Student()
    {
        var auth = RequireStudent();
        if (auth != null) return auth;

        var studentUser = await _db.StudentUsers
            .Include(s => s.ClassroomStudents)
                .ThenInclude(cs => cs.Classroom)
                    .ThenInclude(c => c.Teacher)
                        .ThenInclude(t => t.User)
            .Include(s => s.ClassroomStudents)
                .ThenInclude(cs => cs.Classroom)
                    .ThenInclude(c => c.Class)
            .FirstOrDefaultAsync(s => s.UserId == CurrentUserId);

        if (studentUser == null) return RedirectToAction("Index", "Account");

        return View(new StudentDashboardViewModel
        {
            Classrooms = studentUser.ClassroomStudents.Select(cs => cs.Classroom).ToList(),
            StudentUserId = studentUser.Id
        });
    }
}
