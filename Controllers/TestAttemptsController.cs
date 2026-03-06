using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Domain.Model;
using FrenchLearningPlatform.Infrastructure;

namespace French_Learning_Platform.Controllers;

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
            .AsQueryable();

        if (testId.HasValue)
        {
            attempts = attempts.Where(ta => ta.TestId == testId);
            var test = await _context.Tests.FindAsync(testId.Value);
            ViewBag.TestId = testId;
            ViewBag.TestTitle = test?.Title;
        }

        return View(await attempts.ToListAsync());
    }

    // GET: TestAttempts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var attempt = await _context.TestAttempts
            .Include(ta => ta.Test)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attempt == null) return NotFound();

        return View(attempt);
    }

    // GET: TestAttempts/Create?testId=5
    public IActionResult Create(int? testId)
    {
        ViewBag.Tests = new SelectList(_context.Tests, "Id", "Title", testId);
        ViewBag.ReturnTestId = testId;
        return View();
    }

    // POST: TestAttempts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TestId,Score,MistakesJsonb")] TestAttempt attempt)
    {
        // Load Test nav property so ModelState passes
        if (attempt.TestId.HasValue)
        {
            attempt.Test = await _context.Tests.FindAsync(attempt.TestId.Value);
            ModelState.Remove(nameof(attempt.Test));
        }
        // UserId is not required when auth is not implemented
        ModelState.Remove(nameof(attempt.User));
        ModelState.Remove(nameof(attempt.UserId));

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
    public async Task<IActionResult> Edit(int id, [Bind("Id,TestId,Score,MistakesJsonb,CompletedAt")] TestAttempt attempt)
    {
        if (id != attempt.Id) return NotFound();

        if (attempt.TestId.HasValue)
        {
            attempt.Test = await _context.Tests.FindAsync(attempt.TestId.Value);
            ModelState.Remove(nameof(attempt.Test));
        }
        ModelState.Remove(nameof(attempt.User));
        ModelState.Remove(nameof(attempt.UserId));

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
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var attempt = await _context.TestAttempts
            .Include(ta => ta.Test)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attempt == null) return NotFound();

        return View(attempt);
    }

    // POST: TestAttempts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
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
