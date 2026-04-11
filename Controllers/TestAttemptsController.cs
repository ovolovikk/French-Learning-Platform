using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    // GET: TestAttempts/Create?testId=5
    [Authorize(Roles = AppRoles.Teacher)]
    public IActionResult Create(int? testId)
    {
        ViewBag.Tests = new SelectList(_context.Tests, "Id", "Title", testId);
        ViewBag.ReturnTestId = testId;
        return View();
    }

    // POST: TestAttempts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Create([Bind("TestId,Score,MistakesJsonb")] TestAttempt attempt)
    {
        if (ModelState.IsValid)
        {
            attempt.CompletedAt = DateTime.UtcNow;
            _context.Add(attempt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = attempt.Id });
        }

        ViewBag.Tests = new SelectList(_context.Tests, "Id", "Title", attempt.TestId);
        return View(attempt);
    }

    // GET: TestAttempts/Edit/5
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var attempt = await _context.TestAttempts.FindAsync(id);
        if (attempt == null) return NotFound();

        ViewBag.Tests = new SelectList(_context.Tests, "Id", "Title", attempt.TestId);
        return View(attempt);
    }

    // POST: TestAttempts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TestId,Score,MistakesJsonb,CompletedAt")] TestAttempt attempt)
    {
        if (id != attempt.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(attempt);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttemptExists(attempt.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Details), new { id = attempt.Id });
        }

        ViewBag.Tests = new SelectList(_context.Tests, "Id", "Title", attempt.TestId);
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

    private bool AttemptExists(int id) =>
        _context.TestAttempts.Any(e => e.Id == id);
}
