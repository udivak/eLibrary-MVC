using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.Extensions.Logging;
using static eLibrary.Controllers.UserController;
using PaypalServerSdk;
using PayPal;

namespace eLibrary.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;

    public HomeController(ILogger<HomeController> logger, DB_context dbContext, IHttpContextAccessor context)
    {
        _logger = logger;
        _dbContext = dbContext;
        _context = context;
    }
    
    public IActionResult Index()        // Home Page
    {
        var userName = HttpContext.Session.GetString("userName");

        List<Book> featuredBooks = _dbContext.GetAllBooks().Take(8).ToList();
        
        
        
        return View("Index", featuredBooks);
    }
    
    public IActionResult Privacy()
    {
        return View("Privacy");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}