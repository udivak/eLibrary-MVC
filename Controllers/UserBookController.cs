using Microsoft.EntityFrameworkCore;

namespace eLibrary.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using eLibrary.Models;

using System.Linq;

public class UserBookController : Controller
{
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;

    public UserBookController(IHttpContextAccessor context, DB_context dbContext)
    {
        _dbContext = dbContext;
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> CleanupExpiredBorrowedBooks()
    {
        var expiredBooks = await  _dbContext.UserBook
            .Where(ub => !ub.IsPurchased && ub.BorrowExpiryDate < DateTime.Now)
            .ToListAsync();

        _dbContext.UserBook.RemoveRange(expiredBooks);
        await _dbContext.SaveChangesAsync();

        return Ok($"{expiredBooks.Count} expired borrowed books removed.");
    }
}
