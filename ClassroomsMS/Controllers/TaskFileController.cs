using ClassroomsMS.Data;
using ClassroomsMS.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Controllers;

public class TaskFileController : BaseController
{
    private readonly AppDbContext _db;

    public TaskFileController(AppDbContext db) { _db = db; }

    [HttpPost]
    public async Task<IActionResult> Upload(int assignmentId, IFormFile file)
    {
        var auth = RequireStudent();
        if (auth != null) return auth;

        if (file == null || file.Length == 0)
        {
            TempData["UploadError"] = "Please select a file.";
            var a = await _db.Assignments.FindAsync(assignmentId);
            return RedirectToAction("Tasks", "Classroom", new { id = a!.ClassroomId });
        }

        var studentUser = await _db.StudentUsers.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
        if (studentUser == null) return BadRequest();

        // Replace existing submission if present
        var existing = await _db.TaskFiles
            .FirstOrDefaultAsync(tf => tf.AssignmentId == assignmentId && tf.StudentUserId == studentUser.Id);
        if (existing != null) _db.TaskFiles.Remove(existing);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        _db.TaskFiles.Add(new TaskFile
        {
            AssignmentId = assignmentId,
            StudentUserId = studentUser.Id,
            Data = ms.ToArray(),
            ContentType = file.ContentType,
            FileName = file.FileName,
            UploadedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var assignment = await _db.Assignments.FindAsync(assignmentId);
        return RedirectToAction("Tasks", "Classroom", new { id = assignment!.ClassroomId });
    }

    public async Task<IActionResult> Download(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var taskFile = await _db.TaskFiles.FindAsync(id);
        if (taskFile == null) return NotFound();

        return File(taskFile.Data, taskFile.ContentType, taskFile.FileName);
    }
}
