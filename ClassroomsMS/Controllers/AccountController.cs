using ClassroomsMS.Data;
using ClassroomsMS.Models.Entities;
using ClassroomsMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db) { _db = db; }

    public IActionResult Index() => View();

    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());
        HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");

        return user.Role == UserRole.Teacher
            ? RedirectToAction("Teacher", "Dashboard")
            : RedirectToAction("Student", "Dashboard");
    }

    public IActionResult Signup() => View(new SignupViewModel());

    [HttpPost]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Passwords do not match.");
            return View(model);
        }

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already in use.");
            return View(model);
        }

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password,
            Role = UserRole.Student
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.StudentUsers.Add(new StudentUser { UserId = user.Id });
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", "Student");
        HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");

        return RedirectToAction("Student", "Dashboard");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
