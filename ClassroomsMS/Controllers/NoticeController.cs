using ClassroomsMS.Data;
using ClassroomsMS.Models.Entities;
using ClassroomsMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Controllers;

public class NoticeController : BaseController
{
    private readonly AppDbContext _db;

    public NoticeController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> Create(int classroomId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var classroom = await _db.Classrooms.Include(c => c.Class).FirstOrDefaultAsync(c => c.Id == classroomId);
        if (classroom == null) return NotFound();

        return View(new NoticeFormViewModel
        {
            ClassroomId = classroomId,
            ClassroomName = $"{classroom.Class.Grade}-{classroom.Class.ClassNumber} {classroom.Subject}"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(NoticeFormViewModel model, IFormFileCollection images)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        if (!ModelState.IsValid) return View(model);

        var notice = new Notice
        {
            ClassroomId = model.ClassroomId,
            Text = model.Text,
            CreatedAt = DateTime.UtcNow
        };
        _db.Notices.Add(notice);
        await _db.SaveChangesAsync();

        foreach (var image in images)
        {
            if (image.Length > 0)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                _db.NoticeImages.Add(new NoticeImage
                {
                    NoticeId = notice.Id,
                    Data = ms.ToArray(),
                    ContentType = image.ContentType,
                    FileName = image.FileName
                });
            }
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Notices", "Classroom", new { id = model.ClassroomId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var notice = await _db.Notices
            .Include(n => n.Classroom).ThenInclude(c => c.Class)
            .Include(n => n.Images)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notice == null) return NotFound();

        return View(new NoticeFormViewModel
        {
            Id = notice.Id,
            ClassroomId = notice.ClassroomId,
            Text = notice.Text,
            ClassroomName = $"{notice.Classroom.Class.Grade}-{notice.Classroom.Class.ClassNumber} {notice.Classroom.Subject}",
            ExistingImages = notice.Images.ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(NoticeFormViewModel model, IFormFileCollection images)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        if (!ModelState.IsValid) return View(model);

        var notice = await _db.Notices.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == model.Id);
        if (notice == null) return NotFound();

        notice.Text = model.Text;

        foreach (var image in images)
        {
            if (image.Length > 0)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                _db.NoticeImages.Add(new NoticeImage
                {
                    NoticeId = notice.Id,
                    Data = ms.ToArray(),
                    ContentType = image.ContentType,
                    FileName = image.FileName
                });
            }
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Notices", "Classroom", new { id = notice.ClassroomId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var notice = await _db.Notices.FindAsync(id);
        if (notice == null) return NotFound();

        var classroomId = notice.ClassroomId;
        _db.Notices.Remove(notice);
        await _db.SaveChangesAsync();

        return RedirectToAction("Notices", "Classroom", new { id = classroomId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int imageId, int noticeId)
    {
        var auth = RequireTeacher();
        if (auth != null) return auth;

        var image = await _db.NoticeImages.FindAsync(imageId);
        if (image != null)
        {
            _db.NoticeImages.Remove(image);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Edit", new { id = noticeId });
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int noticeId, string text)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        if (string.IsNullOrWhiteSpace(text))
            return RedirectToAction("Notices", "Classroom");

        _db.NoticeComments.Add(new NoticeComment
        {
            NoticeId = noticeId,
            UserId = CurrentUserId!.Value,
            Text = text,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var notice = await _db.Notices.FindAsync(noticeId);
        return RedirectToAction("Notices", "Classroom", new { id = notice!.ClassroomId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var comment = await _db.NoticeComments.FindAsync(id);
        if (comment == null) return NotFound();

        if (comment.UserId != CurrentUserId && !IsTeacher) return Forbid();

        var notice = await _db.Notices.FindAsync(comment.NoticeId);
        var classroomId = notice!.ClassroomId;

        _db.NoticeComments.Remove(comment);
        await _db.SaveChangesAsync();

        return RedirectToAction("Notices", "Classroom", new { id = classroomId });
    }

    public async Task<IActionResult> Image(int id)
    {
        var image = await _db.NoticeImages.FindAsync(id);
        if (image == null) return NotFound();
        return File(image.Data, image.ContentType);
    }
}
