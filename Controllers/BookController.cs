using System;
using System.Text.Json.Serialization;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;

namespace eLibrary.Controllers;
public class BookController : Controller
{
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;
    public BookController(DB_context dbContext, IHttpContextAccessor context)
    {
        _dbContext = dbContext;
        _context = context;
    }
    private ISession Session => _context.HttpContext.Session;
    
    public async Task<IActionResult> BookDetails(string isbn)
    {
        var book = _dbContext.Books.FirstOrDefault(b => b.ISBN == isbn);
        if (book == null)
        {
            return NotFound();
        }

        string currentUser = Session.GetString("userEmail");
        if (!string.IsNullOrEmpty(currentUser))
        {
            var user_book =
                _dbContext.UserBook.FirstOrDefault(ub => ub.UserEmail == currentUser && ub.BookISBN == book.ISBN);
            if (user_book != null)
            {
                ViewBag.RatingFlag = true;
            }
        }

        var reviews = await _dbContext.GetBookReviewsAsync(book.ISBN);
        BookDetailsViewModel bookDetailsViewModel= new BookDetailsViewModel(book, reviews);
        
        return View("BookDetails", bookDetailsViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> BookReviewSubmit(BookReview bookReview)
    {
        bookReview.CreatedAt = DateTime.Today.ToString("d");
        bookReview.UserName = Session.GetString("userName");
        try
        {
            _dbContext.BookReviews.Add(bookReview);
            await _dbContext.SaveChangesAsync();
            TempData["BookReviewMSG"] = "SUCCESS";
        }
        catch (DbUpdateException ex)                      // current user already posted review for specific book
        {
            TempData["BookReviewMSG"] = "FAIL";
            var currentBook = _dbContext.Books.FirstOrDefault(b => b.ISBN == bookReview.ISBN);
            if (currentBook != null)
                TempData["CurrentBookTitle"] = currentBook.Title;
        }
        return RedirectToAction("Index", "Home");
    }

    public IActionResult EditBook(Book editBook)
    {
        return View("AddBook", editBook);
    }
    
    public IActionResult ConfirmAddBook(Book book)
    {
        return View("ConfirmAddBook", book);
    }
    
    public IActionResult AddBook()
    {
        return View("AddBook", new Book());
    }

    public async Task<IActionResult> BooksGallery(string sortBy, string genre, int? year)
    {
        var books = await _dbContext.GetAllBooksAsync();
        if (string.IsNullOrEmpty(sortBy))                   // No sorting selected, return books as is (default order)
        {
            
            return View("BooksGallery", books);
        }
        switch (sortBy)                             // Sorting logic
        {
            case "priceAsc":
                books = books.OrderBy(b => b.BuyPrice).ToList();
                break;
            case "priceDesc":
                books = books.OrderByDescending(b => b.BuyPrice).ToList();
                break;
            case "mostPopular":
                books = books.FindAll(b => b.isPopular == 1).ToList();
                break;
            case "genre":
                if (!string.IsNullOrEmpty(genre))
                {
                    Genre bookGenre = (Genre)Enum.Parse(typeof(Genre), genre, true);
                    books = books.Where(b => b.Genre == bookGenre).ToList();
                }
                break;
            case "year":
                if (year.HasValue)
                {
                    books = books.Where(b => b.Year == year.Value).ToList();
                }
                break;
            default:
                books = books.OrderBy(b => b.Title).ToList();       // Default sorting by title
                break;
        }
        return View("BooksGallery", books);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBookToLibrary(Book book)
    {
        if (ModelState.IsValid)
        {
            await _dbContext.Books.AddAsync(book);     
            await _dbContext.SaveChangesAsync(); 
            return RedirectToAction("BookAdded", new { isbn = book.ISBN });   //Success Message
        }
        return View("AddBook", book);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBook(string deletedBookISBN)
    {
        var deletedBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == deletedBookISBN);
        if (deletedBook == null)
        {
            TempData["DeleteBookMessage"] = "FAIL";
            return RedirectToAction("Index", "Home");
        }
        var deletedBookReviews = _dbContext.GetBookReviewsAsync(deletedBookISBN).Result;
        var userBooks = _dbContext.GetAllUserBookAsync(deletedBookISBN).Result;
        var waitingLists = _dbContext.GetAllWaitingListAsync(deletedBookISBN).Result;
        
        _dbContext.RemoveRange(deletedBookReviews);
        _dbContext.RemoveRange(userBooks);
        _dbContext.RemoveRange(waitingLists);
        _dbContext.Remove(deletedBook);
        
        await _dbContext.SaveChangesAsync();
        return Ok(new { deletedBook = "null", title = deletedBook.Title });
    }

    public IActionResult DeleteBookPage(Book deletedBook, string title)
    {
        if (string.IsNullOrEmpty(deletedBook.Title))
        {
            deletedBook.Title = title;
        }
        return View("BookDeleted", deletedBook);
    }

    [HttpPatch]
    public async Task<IActionResult> TogglePopular(string isbn)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        string msg;
        if (book == null)
        {
            return NotFound(new { msg = "Book not found, please try again."});
        }
        if (book.isPopular == 1)
        {
            book.isPopular = 0;
            msg = " has been marked as not popular.";
        }
        else
        {
            book.isPopular = 1;
            msg = " has been marked as popular.";
        }
        await _dbContext.SaveChangesAsync();
        return Ok(new { title = book.Title, msg });
    }
    
    [HttpPatch]
    public async Task<IActionResult> ToggleOnSale(string isbn)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        string msg;
        if (book == null)
        {
            return NotFound(new { msg = "Book not found, please try again."});
        }
        if (book.isOnSale == 1)
        {
            book.isOnSale = 0;
            msg = " has been marked as not on sale.";
        }
        else
        {
            book.isOnSale = 1;
            book.isOnSaleDate = DateTime.Now;
            book.isOnSaleExpiryDate = DateTime.Now.AddDays(7);
            msg = " has been marked as on sale.";
        }
        await _dbContext.SaveChangesAsync();
        return Ok(new { title = book.Title, msg });
    }
    
    [HttpGet]
    public async Task<IActionResult> FindABook(string title, string author, string publisher, int? year, string format, string sale, string genre)
    {
        IQueryable<Book> query = _dbContext.Books;
        // Apply filters based on the user's input
        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(b => b.Title.Contains(title));
        }
        if (!string.IsNullOrEmpty(author))
        {
            query = query.Where(b => b.Author.Contains(author));
        }
        if (!string.IsNullOrEmpty(publisher))
        {
            query = query.Where(b => b.Publisher.Contains(publisher));
        }
        if (year.HasValue)
        {
            query = query.Where(b => b.Year == year);
        }
        if (!string.IsNullOrEmpty(format))
        {
            query = query.Where(b => b.Format.Contains(format));
        }
        if (!string.IsNullOrEmpty(sale))
        {
            if (sale == "yes")
            {
                query = query.Where(b => b.isOnSale == 1);
            }
            else if (sale == "no")
            {
                query = query.Where(b => b.isOnSale == 0);
            }
        }
        if (!string.IsNullOrEmpty(genre))
        {
            Genre bookGenre = (Genre)Enum.Parse(typeof(Genre), genre, true);
            query = query.Where(b => b.Genre == bookGenre);
        }
        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(author) && string.IsNullOrEmpty(publisher)
            && !year.HasValue && string.IsNullOrEmpty(format) && string.IsNullOrEmpty(sale) && string.IsNullOrEmpty(genre))
        {
            List<Book> allBooks = await _dbContext.GetAllBooksAsync();
            return View("FindABook",allBooks);
        }
        var result = await query.ToListAsync();
        return View("FindABook", result);
    }
    
    [HttpGet]
    public async Task<IActionResult> BookAdded(string isbn)
    {
        var addedBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        if (addedBook == null)             // If the book is not found in DB
        {
            return RedirectToAction("Index", "Home");
        }
        return View("BookAdded", addedBook);
    }
    
    [HttpGet]
    public async Task<IActionResult> IsAvailable(string isbn)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        if (book == null)
        {
            return Json(new { available = false });
        }
        bool isAvailable = await _dbContext.UserBook.CountAsync(ub => ub.BookISBN == isbn && !ub.IsPurchased) < 3;
        return Json(new { available = isAvailable });
    }
    public async Task<IActionResult> AddToCart(string isbn, string cartAction,bool joinWaitlist)
    {
        if (!joinWaitlist)
        {
            var userEmail = _context.HttpContext.Session.GetString("userEmail");
            var waitingListItem = new WaitingList(isbn, userEmail, 1);
            await _dbContext.WaitingLists.AddAsync(waitingListItem);
            await _dbContext.SaveChangesAsync();
        }
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        if (book == null)
            return RedirectToAction("Error", "Home");
        TempData["BookTitle"] = $"{book.Title}";
        int price;
        if (cartAction == "Buy")
        {
            price = book.BuyPrice;
        }
        else
        {
            price = book.BorrowPrice;
        }

        bool isOnSale = book.isOnSale == 1;
        CartItem addItem = new CartItem(book.ISBN, book.Title, book.Author, cartAction, price, isOnSale, book.SalePercentage);
        ShoppingCart.Add(addItem);
        TempData["AddToCartMessage"] = "SUCCESS";
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> RemoveFromCart(string isbn)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        if (book == null)
            return RedirectToAction("Error", "Home");
        TempData["BookTitle"] = $"'{book.Title}'";
        var shoppingCart = ShoppingCart.GetShoppingCart();
        var removedItem = shoppingCart.Find(item => item.ISBN == isbn);
        if (removedItem == null)
            return RedirectToAction("Error", "Home");
        
        ShoppingCart.Remove(removedItem);
        TempData["RemoveFromCartMessage"] = " has been removed from cart.";
        return RedirectToAction("CheckoutPage", "Checkout");
    }
    
    public async Task<IActionResult> DownloadPdf(string isbn)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        if (book == null)
        {
            return NotFound();
        }
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new PdfWriter(memoryStream))
            {
                using (var pdfDocument = new PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdfDocument);
                    
                    document.Add(new Paragraph($"Book Title: {book.Title}"));
                    document.Add(new Paragraph($"Author: {book.Author}"));
                    document.Add(new Paragraph($"ISBN: {book.ISBN}"));
                    document.Add(new Paragraph($"Publisher: {book.Publisher}"));
                    document.Add(new Paragraph($"Year: {book.Year}"));
                    document.Add(new Paragraph($"Genre: {book.Genre}"));
                    document.Add(new Paragraph($"Pre: You Need To Buy The Book First!!!"));

                    document.Close();
                }
            }
            byte[] pdfBytes = memoryStream.ToArray();
        
            return File(pdfBytes, "application/pdf", $"{book.Title}.pdf");
        }
    }
}