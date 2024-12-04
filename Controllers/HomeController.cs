using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.Extensions.Logging;
using static eLibrary.Controllers.UserController;

namespace eLibrary.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private DB_context _dbContext;

    public HomeController(ILogger<HomeController> logger, DB_context dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public IActionResult Index()        // Home Page
    {
        List<Book> featuredBooks = _dbContext.GetAllBooks().Take(8).ToList();
        return View("Index", featuredBooks);
    }
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
}