using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Domain.Model;
using FrenchLearningPlatform.Infrastructure;

namespace French_Learning_Platform.Controllers;

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
        var userId = await GetOrCreateGuestUserIdAsync();

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
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
        var userId = await GetOrCreateGuestUserIdAsync();

        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.WordId == wordId);

        if (existing != null)
        {
            _context.Favorites.Remove(existing);
            TempData["FavMsg"] = "⭐ Видалено з улюблених.";
        }
        else
        {
            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
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

    /// <summary>
    /// Returns the first user's Id, creating a guest system user if DB is empty.
    /// TODO: REPLACE LATER WITH AUTHENTIFICATED USER
    /// </summary>
    private async Task<int> GetOrCreateGuestUserIdAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync();
        if (user != null) return user.Id;

        // No users yet — create a guest placeholder
        user = new User
        {
            Email = "guest@system.local",
            PasswordHash = "placeholder",
            Role = "Student",
            CreatedAt = DateTime.UtcNow,
            IsEmailConfirmed = false
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }
}
