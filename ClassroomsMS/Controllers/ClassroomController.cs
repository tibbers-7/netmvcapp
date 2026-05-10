using ClassroomsMS.Data;
using ClassroomsMS.Models.Entities;
using ClassroomsMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Controllers;

public class ClassroomController : BaseController
{
    private readonly AppDbContext _db;

    public ClassroomController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        return View(new CreateClassroomViewModel
        {
            Classes = await _db.Classes.ToListAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassroomViewModel model)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        if (!ModelState.IsValid)
        {
            model.Classes = await _db.Classes.ToListAsync();
            return View(model);
        }

        var teacherUser = await _db.TeacherUsers.FirstOrDefaultAsync(t => t.UserId == CurrentUserId);
        if (teacherUser == null) return BadRequest();

        var classroom = new Classroom
        {
            ClassId = model.ClassId,
            Subject = model.Subject,
            TeacherId = teacherUser.Id,
            Code = GenerateCode()
        };
        _db.Classrooms.Add(classroom);
        await _db.SaveChangesAsync();

        return RedirectToAction("Notices", new { id = classroom.Id });
    }

    public async Task<IActionResult> Notices(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms
            .Include(c => c.Class)
            .Include(c => c.Teacher).ThenInclude(t => t.User)
            .Include(c => c.Notices).ThenInclude(n => n.Images)
            .Include(c => c.Notices).ThenInclude(n => n.Comments).ThenInclude(nc => nc.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null) return NotFound();
        if (!await CanAccessClassroom(id)) return Forbid();

        var noticesSorted = classroom.Notices.OrderByDescending(n => n.CreatedAt).ToList();
        classroom.Notices = noticesSorted;

        return View(new ClassroomNoticesViewModel
        {
            Classroom = classroom,
            IsTeacher = IsTeacher,
            CurrentUserId = CurrentUserId!.Value
        });
    }

    public async Task<IActionResult> Tasks(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms
            .Include(c => c.Class)
            .Include(c => c.Teacher).ThenInclude(t => t.User)
            .Include(c => c.Assignments).ThenInclude(a => a.Attachments)
            .Include(c => c.Assignments).ThenInclude(a => a.TaskFiles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null) return NotFound();
        if (!await CanAccessClassroom(id)) return Forbid();

        int? studentUserId = null;
        if (IsStudent)
        {
            var su = await _db.StudentUsers.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            studentUserId = su?.Id;
        }

        int totalStudents = await _db.ClassroomStudents.CountAsync(cs => cs.ClassroomId == id);

        return View(new ClassroomTasksViewModel
        {
            Classroom = classroom,
            IsTeacher = IsTeacher,
            StudentUserId = studentUserId,
            TotalStudents = totalStudents
        });
    }

    public async Task<IActionResult> Students(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms
            .Include(c => c.Class)
            .Include(c => c.Teacher).ThenInclude(t => t.User)
            .Include(c => c.ClassroomStudents)
                .ThenInclude(cs => cs.StudentUser)
                    .ThenInclude(s => s.User)
            .Include(c => c.ClassroomStudents)
                .ThenInclude(cs => cs.StudentUser)
                    .ThenInclude(s => s.Class)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null) return NotFound();

        var teacherUser = await _db.TeacherUsers.FirstOrDefaultAsync(t => t.UserId == CurrentUserId);
        if (classroom.TeacherId != teacherUser?.Id) return Forbid();

        var enrolledIds = classroom.ClassroomStudents.Select(cs => cs.StudentUserId).ToHashSet();
        var availableStudents = await _db.StudentUsers
            .Include(s => s.User)
            .Include(s => s.Class)
            .Where(s => !enrolledIds.Contains(s.Id))
            .ToListAsync();

        return View(new ClassroomStudentsViewModel
        {
            Classroom = classroom,
            AllClasses = await _db.Classes.ToListAsync(),
            AvailableStudents = availableStudents
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent(int classroomId, int studentUserId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var exists = await _db.ClassroomStudents
            .AnyAsync(cs => cs.ClassroomId == classroomId && cs.StudentUserId == studentUserId);

        if (!exists)
        {
            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroomId, StudentUserId = studentUserId });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Students", new { id = classroomId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveStudent(int classroomId, int studentUserId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var cs = await _db.ClassroomStudents
            .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentUserId == studentUserId);

        if (cs != null)
        {
            _db.ClassroomStudents.Remove(cs);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Students", new { id = classroomId });
    }

    [HttpPost]
    public async Task<IActionResult> RegenerateCode(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms.FindAsync(id);
        if (classroom == null) return NotFound();

        var teacherUser = await _db.TeacherUsers.FirstOrDefaultAsync(t => t.UserId == CurrentUserId);
        if (classroom.TeacherId != teacherUser?.Id) return Forbid();

        classroom.Code = GenerateCode();
        await _db.SaveChangesAsync();

        return RedirectToAction("Students", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Join(string code)
    {
        var auth = RequireStudent();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms.FirstOrDefaultAsync(c => c.Code == code);
        if (classroom == null)
        {
            TempData["JoinError"] = "No classroom found with that code.";
            return RedirectToAction("Student", "Dashboard");
        }

        var studentUser = await _db.StudentUsers.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
        if (studentUser == null) return BadRequest();

        var alreadyJoined = await _db.ClassroomStudents
            .AnyAsync(cs => cs.ClassroomId == classroom.Id && cs.StudentUserId == studentUser.Id);

        if (!alreadyJoined)
        {
            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroom.Id, StudentUserId = studentUser.Id });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Notices", new { id = classroom.Id });
    }

    [HttpPost]
    public async Task<IActionResult> AssignClass(int studentUserId, int? classId, int classroomId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var student = await _db.StudentUsers.FindAsync(studentUserId);
        if (student == null) return NotFound();

        student.ClassId = classId;
        await _db.SaveChangesAsync();

        return RedirectToAction("Students", new { id = classroomId });
    }

    private static string GenerateCode() =>
        Guid.NewGuid().ToString("N")[..8].ToUpper();

    private async Task<bool> CanAccessClassroom(int classroomId)
    {
        if (IsTeacher)
        {
            var teacherUser = await _db.TeacherUsers.FirstOrDefaultAsync(t => t.UserId == CurrentUserId);
            return await _db.Classrooms.AnyAsync(c => c.Id == classroomId && c.TeacherId == teacherUser!.Id);
        }
        else
        {
            var studentUser = await _db.StudentUsers.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            return await _db.ClassroomStudents.AnyAsync(cs => cs.ClassroomId == classroomId && cs.StudentUserId == studentUser!.Id);
        }
    }
}
