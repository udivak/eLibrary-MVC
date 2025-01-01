using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        feedback.UserName = Session.GetString("userName");
        feedback.CreatedAt = DateTime.Today.ToString("d");
        try
        {
            _dbContext.eLibraryFeedbacks.Add(feedback);
            await _dbContext.SaveChangesAsync();
            TempData["eLibraryFeedbackMSG"] = "SUCCESS";
        }
        catch (DbUpdateException ex)
        {
            TempData["eLibraryFeedbackMSG"] = "FAIL";
        }
        return RedirectToAction("Index", "Home");
    }
}