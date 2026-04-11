using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrenchLearningPlatform.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using French_Learning_Platform.Security;
using Microsoft.AspNetCore.Authorization;

namespace French_Learning_Platform.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppRoles.Teacher)]
public class ChartsController : ControllerBase
{
    private record TestAttemptCountResponseItem(string Date, int Count);
    private record AverageTestScoreResponseItem(string TestTitle, double AverageScore);

    private readonly FrenchLearningPlatformDbContext _context;

    public ChartsController(FrenchLearningPlatformDbContext context)
    {
        _context = context;
    }

    [HttpGet("attemptsByDate")]
    public async Task<JsonResult> GetAttemptsByDateAsync(CancellationToken cancellationToken)
    {
        // EF Core might have issues grouping by DateTime.Date on some databases,
        // so we fetch first or group by ToShortDateString if supported, but fetching
        // minimal data is safer for cross-db compatibility like SQLite/PostgreSQL.
        var attempts = await _context.TestAttempts
            .Where(ta => ta.CompletedAt != null)
            .Select(ta => new { ta.CompletedAt })
            .ToListAsync(cancellationToken);

        var responseItems = attempts
            .GroupBy(ta => ta.CompletedAt!.Value.Date)
            .Select(group => new TestAttemptCountResponseItem(
                group.Key.ToString("yyyy-MM-dd"),
                group.Count()
            ))
            .OrderBy(x => x.Date)
            .ToList();

        return new JsonResult(responseItems);
    }

    [HttpGet("averageScoreByTest")]
    public async Task<JsonResult> GetAverageScoreByTestAsync(CancellationToken cancellationToken)
    {
        var responseItems = await _context.TestAttempts
            .Include(ta => ta.Test)
            .Where(ta => ta.Test != null && ta.Test.Title != null && ta.Score != null)
            .GroupBy(ta => ta.Test!.Title)
            .Select(group => new AverageTestScoreResponseItem(
                group.Key!,
                group.Average(ta => ta.Score!.Value)
            ))
            .ToListAsync(cancellationToken);

        return new JsonResult(responseItems);
    }
}
