using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Domain.Model;
using FrenchLearningPlatform.Infrastructure;
using French_Learning_Platform.Security;
using Microsoft.AspNetCore.Authorization;

namespace French_Learning_Platform.Controllers;

[Authorize]
public class TestAttemptsController : Controller
{
    private readonly FrenchLearningPlatformDbContext _context;

    public TestAttemptsController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    // GET: TestAttempts
    public async Task<IActionResult> Index(int? testId)
    {
        var attempts = _context.TestAttempts
            .Include(ta => ta.Test)
            .Include(ta => ta.User)
            .AsQueryable();

        if (!User.IsInRole(AppRoles.Teacher))
        {
            var currentUserId = User.GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Challenge();
            }

            attempts = attempts.Where(ta => ta.UserId == currentUserId.Value);
        }

        if (testId.HasValue)
        {
            attempts = attempts.Where(ta => ta.TestId == testId);
            var test = await _context.Tests.FindAsync(testId.Value);
            ViewBag.TestId = testId;
            ViewBag.TestTitle = test?.Title;
        }

        return View(await attempts
            .OrderByDescending(ta => ta.CompletedAt)
            .ToListAsync());
    }

    // GET: TestAttempts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var attempt = await _context.TestAttempts
            .Include(ta => ta.Test)
            .Include(ta => ta.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attempt == null) return NotFound();

        if (!User.IsInRole(AppRoles.Teacher))
        {
            var currentUserId = User.GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Challenge();
            }

            if (attempt.UserId != currentUserId.Value)
            {
                return Forbid();
            }
        }

        return View(attempt);
    }

    // GET: TestAttempts/Delete/5
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var attempt = await _context.TestAttempts
            .Include(ta => ta.Test)
            .Include(ta => ta.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attempt == null) return NotFound();

        return View(attempt);
    }

    // POST: TestAttempts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var attempt = await _context.TestAttempts.FindAsync(id);
        if (attempt != null)
        {
            _context.TestAttempts.Remove(attempt);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
