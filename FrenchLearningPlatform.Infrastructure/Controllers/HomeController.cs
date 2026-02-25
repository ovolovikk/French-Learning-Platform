using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FrenchLearningPlatform.Infrastructure.Models;

namespace FrenchLearningPlatform.Infrastructure.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FrenchLearningPlatformDbContext _context;

    public HomeController(ILogger<HomeController> logger, FrenchLearningPlatformDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return Json(_context.Words.ToList());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
