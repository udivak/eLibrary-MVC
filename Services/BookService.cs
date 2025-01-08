using eLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Services;

public class BookService
{
    private readonly DB_context _dbContext; 

    public BookService(DB_context dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void UpdateExpiredSales()
    {
        var today = DateTime.Today.Date;

        // Get all books where the sale has expired
        var booksToUpdate = _dbContext.Books
            .Where(b => b.isOnSale == 1 && b.isOnSaleExpiryDate.HasValue && b.isOnSaleExpiryDate.Value.Date == today)
            .ToList();

        foreach (var book in booksToUpdate)
        {
            book.isOnSale = 0;
            book.isOnSaleDate = null;
            book.isOnSaleExpiryDate = null;
        }
        
        _dbContext.SaveChanges();
    }
}