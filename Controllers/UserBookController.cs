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
    /*
    // Borrow a book
    [HttpPost]
    public IActionResult BorrowBook(int bookId, int days = 14)
    {
        var userEmail = HttpContext.Session.GetString("userEmail");
        var borrowDate = DateTime.Now;
        var expiryDate = borrowDate.AddDays(days);

        var userBook = new UserBook
        {
            UserEmail = userEmail,
            BookId = bookId,
            BorrowDate = borrowDate,
            BorrowExpiryDate = expiryDate,
            IsPurchased = false
        };
        
        _dbContext.UserBooks.Add(userBook);
        _dbContext.SaveChanges();

        return Ok("Book borrowed successfully.");
    }

    // Purchase a book
    [HttpPost]
    public IActionResult PurchaseBook(string userEmail, int bookId)
    {
        var userBook = new UserBook
        {
            UserEmail = userEmail,
            BookId = bookId,
            PurchaseDate = DateTime.Now,
            IsPurchased = true
        };

        _dbContext.UserBooks.Add(userBook);
        _dbContext.SaveChanges();

        return Ok("Book purchased successfully.");
    }*/

    // Clean up expired borrowed books
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
