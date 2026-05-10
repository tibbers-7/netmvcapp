using Microsoft.AspNetCore.Mvc;

namespace ClassroomsMS.Controllers;

public abstract class BaseController : Controller
{
    protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
    protected string? CurrentUserRole => HttpContext.Session.GetString("UserRole");
    protected string? CurrentUserName => HttpContext.Session.GetString("UserFullName");

    protected bool IsAuthenticated => CurrentUserId.HasValue;
    protected bool IsTeacher => CurrentUserRole == "Teacher";
    protected bool IsStudent => CurrentUserRole == "Student";

    protected IActionResult? RequireAuth()
    {
        if (!IsAuthenticated) return RedirectToAction("Index", "Account");
        return null;
    }

    protected IActionResult? RequireTeacher()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;
        if (!IsTeacher) return Forbid();
        return null;
    }

    protected IActionResult? RequireStudent()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;
        if (!IsStudent) return Forbid();
        return null;
    }
}
