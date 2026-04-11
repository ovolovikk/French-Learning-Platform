using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Infrastructure;
using FrenchLearningPlatform.Domain.Model;
using French_Learning_Platform.Security;
using Microsoft.AspNetCore.Authorization;

namespace French_Learning_Platform.Controllers;

[Authorize(Roles = AppRoles.Student)]
public class FavoritesController : Controller
{
    private readonly FrenchLearningPlatformDbContext _context;

    public FavoritesController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    // GET: Favorites — список усіх улюблених слів
    public async Task<IActionResult> Index()
    {
        var userId = User.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Challenge();
        }

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId.Value)
            .Include(f => f.Word)
                .ThenInclude(w => w!.Category)
            .OrderByDescending(f => f.AddedAt)
            .ToListAsync();

        return View(favorites);
    }

    // POST: Favorites/Toggle/5  — додає або видаляє слово з улюблених
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int wordId, string? returnUrl)
    {
        var userId = User.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Challenge();
        }

        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.WordId == wordId);

        if (existing != null)
        {
            _context.Favorites.Remove(existing);
            TempData["FavMsg"] = "⭐ Видалено з улюблених.";
        }
        else
        {
            _context.Favorites.Add(new Favorite
            {
                UserId = userId.Value,
                WordId = wordId,
                AddedAt = DateTime.UtcNow
            });
            TempData["FavMsg"] = "⭐ Додано до улюблених!";
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Words");
    }
}
