using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Domain.Model;
using FrenchLearningPlatform.Infrastructure;

namespace French_Learning_Platform.Controllers;

public class TestsController : Controller
{
    private readonly FrenchLearningPlatformDbContext _context;

    public TestsController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    private async Task<int> GetOrCreateGuestUserIdAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "guest@example.com");
        if (user == null)
        {
            user = new User
            {
                Email = "guest@example.com",
                PasswordHash = "guest_hash",
                Role = "Student",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        return user.Id;
    }

    // GET: Tests/Take/5
    public async Task<IActionResult> Take(int? id)
    {
        if (id == null) return NotFound();

        var test = await _context.Tests
            .Include(t => t.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (test == null) return NotFound();

        var query = _context.Words.AsQueryable();
        if (test.CategoryId.HasValue)
        {
            query = query.Where(w => w.CategoryId == test.CategoryId);
        }

        int wordsCount = test.Words ?? 10;
        if (wordsCount <= 0) wordsCount = 10;

        var words = await query
            .OrderBy(x => EF.Functions.Random())
            .Take(wordsCount)
            .ToListAsync();

        ViewBag.TestWords = words;
        return View(test);
    }

    // POST: Tests/SubmitTest
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTest(int testId, IFormCollection form)
    {
        var test = await _context.Tests.FindAsync(testId);
        if (test == null) return NotFound();

        int score = 0;
        var mistakes = new List<object>();

        foreach (var key in form.Keys)
        {
            if (key.StartsWith("word_"))
            {
                if (int.TryParse(key.Substring(5), out int wordId))
                {
                    var word = await _context.Words.FindAsync(wordId);
                    if (word != null)
                    {
                        var userTranslation = form[key].ToString()?.Trim() ?? "";
                        var expected = word.Translation?.Trim() ?? "";

                        if (string.Equals(userTranslation, expected, StringComparison.OrdinalIgnoreCase))
                        {
                            score++;
                        }
                        else
                        {
                            mistakes.Add(new
                            {
                                word_id = word.Id,
                                french = word.FrenchTerm,
                                expected_translation = expected,
                                user_translation = userTranslation
                            });
                        }
                    }
                }
            }
        }

        var userId = await GetOrCreateGuestUserIdAsync();

        var attempt = new TestAttempt
        {
            TestId = testId,
            UserId = userId,
            Score = score,
            MistakesJsonb = mistakes.Any() ? System.Text.Json.JsonSerializer.Serialize(mistakes) : null,
            CompletedAt = DateTime.UtcNow
        };

        _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "TestAttempts", new { id = attempt.Id });
    }

    // GET: Tests  OR  Tests?categoryId=5
    public async Task<IActionResult> Index(int? categoryId)
    {
        var tests = _context.Tests.Include(t => t.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            tests = tests.Where(t => t.CategoryId == categoryId);
            var category = await _context.Categories.FindAsync(categoryId.Value);
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = category?.Name;
        }

        return View(await tests.ToListAsync());
    }

    // GET: Tests/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var test = await _context.Tests
            .Include(t => t.Category)
            .Include(t => t.TestAttempts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (test == null) return NotFound();

        return View(test);
    }

    // GET: Tests/Create?categoryId=5
    public IActionResult Create(int? categoryId)
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);
        ViewBag.ReturnCategoryId = categoryId;
        return View();
    }

    // POST: Tests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryId,Words,Title,TimeLimitSeconds")] Test test)
    {
        // Fix: load Category navigation property
        if (test.CategoryId.HasValue)
        {
            test.Category = await _context.Categories.FindAsync(test.CategoryId.Value);
            ModelState.Remove(nameof(test.Category));
        }

        if (ModelState.IsValid)
        {
            _context.Add(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { categoryId = test.CategoryId });
        }

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", test.CategoryId);
        ViewBag.ReturnCategoryId = test.CategoryId;
        return View(test);
    }

    // GET: Tests/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var test = await _context.Tests.FindAsync(id);
        if (test == null) return NotFound();

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", test.CategoryId);
        return View(test);
    }

    // POST: Tests/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Words,Title,TimeLimitSeconds")] Test test)
    {
        if (id != test.Id) return NotFound();

        // Fix: load Category navigation property
        if (test.CategoryId.HasValue)
        {
            test.Category = await _context.Categories.FindAsync(test.CategoryId.Value);
            ModelState.Remove(nameof(test.Category));
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(test);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestExists(test.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", test.CategoryId);
        return View(test);
    }

    // GET: Tests/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var test = await _context.Tests
            .Include(t => t.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (test == null) return NotFound();

        return View(test);
    }

    // POST: Tests/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var test = await _context.Tests.FindAsync(id);
        if (test != null)
        {
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool TestExists(int id)
    {
        return _context.Tests.Any(e => e.Id == id);
    }
}
