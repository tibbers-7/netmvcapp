using ClassroomsMS.Data;
using ClassroomsMS.Models.Entities;
using ClassroomsMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Controllers;

public class AssignmentController : BaseController
{
    private readonly AppDbContext _db;

    public AssignmentController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> Create(int classroomId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms.Include(c => c.Class).FirstOrDefaultAsync(c => c.Id == classroomId);
        if (classroom == null) return NotFound();

        return View(new AssignmentFormViewModel
        {
            ClassroomId = classroomId,
            ClassroomName = $"{classroom.Class.Grade}-{classroom.Class.ClassNumber} {classroom.Subject}"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(AssignmentFormViewModel model, IFormFileCollection files)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        if (!ModelState.IsValid) return View(model);

        var assignment = new Assignment
        {
            ClassroomId = model.ClassroomId,
            Text = model.Text,
            CreatedAt = DateTime.UtcNow
        };
        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                _db.TaskAttachments.Add(new TaskAttachment
                {
                    AssignmentId = assignment.Id,
                    Data = ms.ToArray(),
                    ContentType = file.ContentType,
                    FileName = file.FileName
                });
            }
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Tasks", "Classroom", new { id = model.ClassroomId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var assignment = await _db.Assignments
            .Include(a => a.Classroom).ThenInclude(c => c.Class)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null) return NotFound();

        return View(new AssignmentFormViewModel
        {
            Id = assignment.Id,
            ClassroomId = assignment.ClassroomId,
            Text = assignment.Text,
            ClassroomName = $"{assignment.Classroom.Class.Grade}-{assignment.Classroom.Class.ClassNumber} {assignment.Classroom.Subject}",
            ExistingAttachments = assignment.Attachments.ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AssignmentFormViewModel model, IFormFileCollection files)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        if (!ModelState.IsValid) return View(model);

        var assignment = await _db.Assignments.Include(a => a.Attachments).FirstOrDefaultAsync(a => a.Id == model.Id);
        if (assignment == null) return NotFound();

        assignment.Text = model.Text;

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                _db.TaskAttachments.Add(new TaskAttachment
                {
                    AssignmentId = assignment.Id,
                    Data = ms.ToArray(),
                    ContentType = file.ContentType,
                    FileName = file.FileName
                });
            }
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Tasks", "Classroom", new { id = assignment.ClassroomId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var assignment = await _db.Assignments.FindAsync(id);
        if (assignment == null) return NotFound();

        var classroomId = assignment.ClassroomId;
        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync();

        return RedirectToAction("Tasks", "Classroom", new { id = classroomId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAttachment(int attachmentId, int assignmentId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var attachment = await _db.TaskAttachments.FindAsync(attachmentId);
        if (attachment != null)
        {
            _db.TaskAttachments.Remove(attachment);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Edit", new { id = assignmentId });
    }

    public async Task<IActionResult> File(int id)
    {
        var attachment = await _db.TaskAttachments.FindAsync(id);
        if (attachment == null) return NotFound();
        return File(attachment.Data, attachment.ContentType, attachment.FileName);
    }

    public async Task<IActionResult> Submissions(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var assignment = await _db.Assignments
            .Include(a => a.TaskFiles)
                .ThenInclude(tf => tf.StudentUser)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null) return NotFound();

        int totalStudents = await _db.ClassroomStudents.CountAsync(cs => cs.ClassroomId == assignment.ClassroomId);

        return PartialView("_SubmissionsPartial", new SubmissionsViewModel
        {
            AssignmentId = id,
            Submissions = assignment.TaskFiles.ToList(),
            TotalStudents = totalStudents
        });
    }
}
