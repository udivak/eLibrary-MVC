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
    private ISession Session => _context.HttpContext.Session;
    
    public async Task<IActionResult> Index()        // Home Page
    {
        /*
        List<Book> featuredBooks = await _dbContext.GetAllBooksAsync();
        if (featuredBooks == null)
        {
            featuredBooks = new List<Book>();
            return View("Index", featuredBooks);
        }
        //take most popular ones
        featuredBooks = featuredBooks.Take(15).ToList();
        //
        return View("Index", featuredBooks);*/
        
        List<Book> featuredBooks = await _dbContext.GetAllBooksAsync();
        if (featuredBooks == null)
        {
            featuredBooks = new List<Book>();
        }
        featuredBooks = featuredBooks.Take(15).ToList();
        
        List<eLibraryFeedback> feedbacks = await _dbContext.GetAlleLibraryFeedbacksAsync();
        if (feedbacks == null)
        {
            feedbacks = new List<eLibraryFeedback>();
        }
        
        IndexViewModel IndexLists = new IndexViewModel(featuredBooks, feedbacks);
        return View("Index", IndexLists);
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