using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Domain.Model;
using FrenchLearningPlatform.Infrastructure;

namespace French_Learning_Platform.Controllers;

public class CategoriesController : Controller
{
    private readonly FrenchLearningPlatformDbContext _context;

    public CategoriesController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Words)
            .Include(c => c.Tests)
            .ToListAsync();
        return View(categories);
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories
            .Include(c => c.Words)
            .Include(c => c.Tests)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null) return NotFound();

        ViewBag.ClearResult = TempData["ClearResult"];
        return View(category);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
    {
        if (id != category.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null) return NotFound();

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _context.TestAttempts.Where(ta => ta.Test.CategoryId == id).ExecuteDeleteAsync();
        await _context.Favorites.Where(f => f.Word.CategoryId == id).ExecuteDeleteAsync();
        await _context.Words.Where(w => w.CategoryId == id).ExecuteDeleteAsync();
        await _context.Tests.Where(t => t.CategoryId == id).ExecuteDeleteAsync();

        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Categories/ClearWords/5  — видаляє всі слова категорії
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearWords(int id)
    {
        // Перед видаленням слів, треба видалити їх із улюблених
        await _context.Favorites.Where(f => f.Word.CategoryId == id).ExecuteDeleteAsync();

        var count = await _context.Words
            .Where(w => w.CategoryId == id)
            .ExecuteDeleteAsync();

        TempData["ClearResult"] = $"🗑 Видалено {count} слів з категорії.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}
