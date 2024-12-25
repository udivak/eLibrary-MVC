using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Controllers;

public class eLibraryFeedbackController : Controller
{
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;

    public eLibraryFeedbackController(DB_context dbContext, IHttpContextAccessor context)
    {
        _dbContext = dbContext;
        _context = context;
    }
    private ISession Session => _context.HttpContext.Session;
    
    public IActionResult eLibraryFeedback()
    {
        return View("eLibraryFeedback", new eLibraryFeedback());
    }
    
    public async Task<IActionResult> eLibraryFeedbackSubmit(eLibraryFeedback feedback)
    {
        _dbContext.eLibraryFeedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync();
        TempData["eLibraryFeedbackMSG"] = "SUCCESS";
        return RedirectToAction("Index", "Home");
    }
}