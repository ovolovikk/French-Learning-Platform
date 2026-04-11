using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using FrenchLearningPlatform.Domain.Model;
using FrenchLearningPlatform.Infrastructure;
using French_Learning_Platform.Security;
using Microsoft.AspNetCore.Authorization;

namespace French_Learning_Platform.Controllers;

[Authorize]
public class WordsController : Controller
{
    private readonly FrenchLearningPlatformDbContext _context;

    public WordsController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    // GET: Words  OR  Words?categoryId=5
    public async Task<IActionResult> Index(int? categoryId, string? searchString, int? difficulty)
    {
        var words = _context.Words.Include(w => w.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            words = words.Where(w => w.CategoryId == categoryId);
            var category = await _context.Categories.FindAsync(categoryId.Value);
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = category?.Name;
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            words = words.Where(w => w.FrenchTerm!.Contains(searchString) || w.Translation!.Contains(searchString));
            ViewBag.SearchString = searchString;
        }

        if (difficulty.HasValue)
        {
            words = words.Where(w => w.DifficultyLevel == difficulty.Value);
            ViewBag.Difficulty = difficulty;
        }

        // Favorite button is available only for students.
        ViewBag.CanUseFavorites = User.IsInRole(AppRoles.Student);
        if (User.IsInRole(AppRoles.Student))
        {
            var currentUserId = User.GetCurrentUserId();
            if (currentUserId.HasValue)
            {
                var favIds = await _context.Favorites
                    .Where(f => f.UserId == currentUserId.Value)
                    .Select(f => f.WordId)
                    .ToHashSetAsync();
                ViewBag.FavoriteWordIds = favIds;
            }
            else
            {
                ViewBag.FavoriteWordIds = new HashSet<int>();
            }
        }
        else
        {
            ViewBag.FavoriteWordIds = new HashSet<int>();
        }

        // Show import result message if redirected from ImportCsv
        ViewBag.ImportResult = TempData["ImportResult"];

        words = words.OrderBy(w => w.Category!.Name).ThenBy(w => w.FrenchTerm);

        return View(await words.ToListAsync());
    }

    // GET: Words/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var word = await _context.Words
            .Include(w => w.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (word == null) return NotFound();

        return View(word);
    }

    // GET: Words/Create  OR  Words/Create?categoryId=5
    [Authorize(Roles = AppRoles.Teacher)]
    public IActionResult Create(int? categoryId)
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);
        ViewBag.ReturnCategoryId = categoryId;
        return View();
    }

    // POST: Words/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Create([Bind("CategoryId,FrenchTerm,Translation,DifficultyLevel")] Word word)
    {
        if (ModelState.IsValid)
        {
            _context.Add(word);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { categoryId = word.CategoryId });
        }

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", word.CategoryId);
        ViewBag.ReturnCategoryId = word.CategoryId;
        return View(word);
    }

    // GET: Words/Edit/5
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var word = await _context.Words.FindAsync(id);
        if (word == null) return NotFound();

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", word.CategoryId);
        return View(word);
    }

    // POST: Words/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,FrenchTerm,Translation,DifficultyLevel")] Word word)
    {
        if (id != word.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(word);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WordExists(word.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", word.CategoryId);
        return View(word);
    }

    // GET: Words/Delete/5
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var word = await _context.Words
            .Include(w => w.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (word == null) return NotFound();

        return View(word);
    }

    // POST: Words/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _context.Favorites
            .Where(f => f.WordId == id)
            .ExecuteDeleteAsync();

        await _context.Words
            .Where(w => w.Id == id)
            .ExecuteDeleteAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Words/ImportCsv
    [Authorize(Roles = AppRoles.Teacher)]
    public IActionResult ImportCsv(int? categoryId)
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);
        ViewBag.ReturnCategoryId = categoryId;
        return View();
    }

    // POST: Words/ImportCsv
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> ImportCsv(int? categoryId, IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
            ModelState.AddModelError("csvFile", "Будь ласка, виберіть CSV-файл.");
        if (!categoryId.HasValue)
            ModelState.AddModelError("categoryId", "Будь ласка, оберіть категорію.");

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);
            return View();
        }

        var words = new List<Word>();
        var skipped = 0;
        bool isFirstLine = true;

        using var reader = new StreamReader(csvFile!.OpenReadStream(), detectEncodingFromByteOrderMarks: true);
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            // strip null chars that appear when file is UTF-16 read partially wrong
            line = line.Replace("\0", "");
            if (string.IsNullOrWhiteSpace(line)) continue;

            var sep = line.Contains(';') ? ';' : ',';
            var parts = line.Split(sep);
            if (parts.Length < 2) { skipped++; continue; }

            var french = parts[0].Trim().Trim('"');
            var translation = parts[1].Trim().Trim('"');

            // Skip header row heuristic
            if (isFirstLine)
            {
                isFirstLine = false;
                bool looksLikeHeader =
                    french.Equals("french", StringComparison.OrdinalIgnoreCase) ||
                    french.Equals("mot", StringComparison.OrdinalIgnoreCase) ||
                    french.Equals("word", StringComparison.OrdinalIgnoreCase) ||
                    french.Equals("слово", StringComparison.OrdinalIgnoreCase);
                if (looksLikeHeader) continue;
            }

            if (string.IsNullOrWhiteSpace(french) || string.IsNullOrWhiteSpace(translation))
            {
                skipped++;
                continue;
            }

            words.Add(new Word
            {
                CategoryId = categoryId!.Value,
                FrenchTerm = french,
                Translation = translation,
                DifficultyLevel = null   // не виставляється при CSV-імпорті
            });
        }

        if (words.Count > 0)
        {
            await _context.Words.AddRangeAsync(words);
            await _context.SaveChangesAsync();
        }

        TempData["ImportResult"] =
            $"✅ Імпортовано {words.Count} слів." +
            (skipped > 0 ? $" Пропущено некоректних рядків: {skipped}." : "");

        return RedirectToAction(nameof(Index), new { categoryId });
    }

    // GET: Words/Export
    [HttpGet]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> Export(
        int? categoryId,
        string? searchString,
        int? difficulty,
        [FromQuery] string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(contentType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Підтримується лише формат .xlsx");
        }

        var wordsQuery = _context.Words
            .AsNoTracking()
            .Include(w => w.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            wordsQuery = wordsQuery.Where(w => w.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            wordsQuery = wordsQuery.Where(w =>
                (w.FrenchTerm != null && w.FrenchTerm.Contains(searchString)) ||
                (w.Translation != null && w.Translation.Contains(searchString)));
        }

        if (difficulty.HasValue)
        {
            wordsQuery = wordsQuery.Where(w => w.DifficultyLevel == difficulty);
        }

        var words = await wordsQuery
            .OrderBy(w => w.Category!.Name)
            .ThenBy(w => w.FrenchTerm)
            .ToListAsync(cancellationToken);

        await using var memoryStream = new MemoryStream();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Words");

            worksheet.Cell(1, 1).Value = "French";
            worksheet.Cell(1, 2).Value = "Translation";
            worksheet.Cell(1, 3).Value = "Difficulty";
            worksheet.Cell(1, 4).Value = "Category";

            worksheet.Row(1).Style.Font.Bold = true;

            var row = 2;
            foreach (var word in words)
            {
                worksheet.Cell(row, 1).Value = word.FrenchTerm ?? string.Empty;
                worksheet.Cell(row, 2).Value = word.Translation ?? string.Empty;
                worksheet.Cell(row, 3).Value = word.DifficultyLevel;
                worksheet.Cell(row, 4).Value = word.Category?.Name ?? string.Empty;
                row++;
            }

            worksheet.Columns(1, 4).AdjustToContents();
            workbook.SaveAs(memoryStream);
        }

        memoryStream.Position = 0;

        var fileName = $"words_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(memoryStream.ToArray(), contentType, fileName);
    }

    private bool WordExists(int id)
    {
        return _context.Words.Any(e => e.Id == id);
    }
}
