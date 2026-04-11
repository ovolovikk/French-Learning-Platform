using FrenchLearningPlatform.Infrastructure;
using French_Learning_Platform.Security;
using French_Learning_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace French_Learning_Platform.Controllers;

[Authorize(Roles = AppRoles.Teacher)]
public class RolesController : Controller
{
    private readonly FrenchLearningPlatformDbContext _context;

    public RolesController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync();

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ChangeRoleViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            SelectedRole = user.Role == AppRoles.Teacher ? AppRoles.Teacher : AppRoles.Student
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ChangeRoleViewModel model)
    {
        if (!model.AvailableRoles.Contains(model.SelectedRole))
        {
            ModelState.AddModelError(nameof(model.SelectedRole), "Недопустима роль.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FindAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        user.Role = model.SelectedRole;
        await _context.SaveChangesAsync();

        TempData["RoleChanged"] = $"Роль для {user.Email} змінено на {model.SelectedRole}.";
        return RedirectToAction(nameof(Index));
    }
}
