using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.Extensions.Logging;
using static eLibrary.Controllers.UserController;

namespace eLibrary.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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

    public IActionResult Register()
    {
        //Create new user
        Models.User user = new Models.User();
        return RedirectToAction( "RegistrationSubmit","UserController", user);
    }
}