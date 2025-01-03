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
            return NotFound(); // Return 404 if the book is not found
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
        
        return View("BookDetails", bookDetailsViewModel); // Pass the book to the view
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
        // Apply sorting based on the chosen option
        if (string.IsNullOrEmpty(sortBy))
        {
            // No sorting selected, return books as is (default order)
            return View("BooksGallery", books);
        }
        // Sorting logic
        switch (sortBy)
        {
            case "priceAsc":
                books = books.OrderBy(b => b.BuyPrice).ToList();
                break;
            case "priceDesc":
                books = books.OrderByDescending(b => b.BuyPrice).ToList();
                break;
            /*case "mostPopular":
                books = books.OrderByDescending(b => b.Popularity).ToList(); // Assuming Popularity is a property
                break;*/
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
                books = books.OrderBy(b => b.Title).ToList(); // Default sorting by title
                break;
        }

        return View("BooksGallery", books);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBookToLibrary(Book book)
    {
        if (ModelState.IsValid)
        {
            _dbContext.Books.Add(book);     // Add the new book to the database
            await _dbContext.SaveChangesAsync(); 
            return RedirectToAction("BookAdded", new { isbn = book.ISBN });   //Success Message
        }
        return View("AddBook", book);
    }

    public IActionResult DeleteBook(Book deletedBook)       //Delete new book without adding to DB
    {
        return View("DeleteBook", deletedBook);
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
    public IActionResult FilterBooks(string title, string author, string genre, string publisher, int? year)
    {
        var books = _dbContext.Books.AsQueryable();
        
        if (!string.IsNullOrEmpty(title))
            books = books.Where(b => b.Title.Contains(title));
        if (!string.IsNullOrEmpty(author))
            books = books.Where(b => b.Author.Contains(author));
        if (!string.IsNullOrEmpty(genre))
        {
            Genre bookGenre = (Genre)Enum.Parse(typeof(Genre), genre, true);
            books = books.Where(b => b.Genre == bookGenre);
        }
        if (!string.IsNullOrEmpty(publisher))
            books = books.Where(b => b.Publisher.Contains(publisher));
        if (year.HasValue)
            books = books.Where(b => b.Year == year.Value);

        return PartialView("FindABook", books.ToList());
    }

    public IActionResult FilterBooksByCategory(string genre)
    {
        Genre bookGenre = (Genre)Enum.Parse(typeof(Genre), genre, true);
        var books = _dbContext.Books.Where(b => b.Genre == bookGenre);
        return PartialView("FindABook", books.ToList());
    }
    
    [HttpGet]
    public IActionResult BookAdded(string isbn)
    {
        var addedBook = _dbContext.Books.FirstOrDefault(b => b.ISBN == isbn);
        if (addedBook == null)             // If the book is not found in DB
        {
            return RedirectToAction("Index", "Home");
        }
        return View("BookAdded", addedBook);
    }
    public async Task<IActionResult> AddToCart(string isbn, string cartAction, string qty)
    {
        int quantity;
        try
        {
            quantity = int.Parse(qty);
        }
        catch (ArgumentNullException)
        {
            quantity = 0;
        }
        var book = _dbContext.Books.FirstOrDefault(b => b.ISBN == isbn);
        if (book == null)
            return RedirectToAction("Error", "Home");
        TempData["BookTitle"] = $"{book.Title}";
        /*if (book.Format == "Physical" && book.Quantity < quantity)                  //No more physical copies
        { 
            TempData["AddToCartMessage"] = "FAIL";
            string currentUser = Session.GetString("userEmail");
            var waitingList = new WaitingList(isbn, currentUser, quantity);
            _dbContext.WaitingLists.Add(waitingList);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }*/
        int price;
        if (cartAction == "Buy")
        {
            price = book.BuyPrice;
        }
        else
        {
            price = book.BorrowPrice;
        }
        CartItem addItem = new CartItem(book.ISBN, book.Title, cartAction, price, quantity);
        ShoppingCart.Add(addItem);
        TempData["AddToCartMessage"] = "SUCCESS";
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> RemoveFromCart(string isbn)
    {
        var book = _dbContext.Books.FirstOrDefault(b => b.ISBN == isbn);
        if (book == null)
            return RedirectToAction("Error", "Home");
        TempData["BookTitle"] = $"'{book.Title}'";
        var shoppingCart = ShoppingCart.GetShoppingCart();
        CartItem removedItem = shoppingCart.Find(item => item.ISBN == isbn);
        int qty = removedItem.Quantity;
        /*if (book.Format == "Physical")
        {
            book.Quantity += qty;
            await _dbContext.SaveChangesAsync();
        }*/
        ShoppingCart.Remove(removedItem);
        TempData["RemoveFromCartMessage"] = " has been removed from cart.";
        return RedirectToAction("CheckoutPage", "Checkout");
    }

    public IActionResult SearchResults()
    {
        return View(_dbContext.Books.ToList());
    }
    public IActionResult DownloadPdf(string isbn)
    {
        var book = _dbContext.Books.FirstOrDefault(b => b.ISBN == isbn);
        if (book == null)
        {
            return NotFound();
        }

        // Create a memory stream to hold the PDF data
        using (var memoryStream = new MemoryStream())
        {
            // Create a PdfWriter instance
            using (var writer = new PdfWriter(memoryStream))
            {
                using (var pdfDocument = new PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdfDocument);

                    // Add content to the PDF document
                    document.Add(new Paragraph($"Book Title: {book.Title}"));
                    document.Add(new Paragraph($"Author: {book.Author}"));
                    document.Add(new Paragraph($"ISBN: {book.ISBN}"));
                    document.Add(new Paragraph($"Publisher: {book.Publisher}"));
                    document.Add(new Paragraph($"Year: {book.Year}"));
                    document.Add(new Paragraph($"Genre: {book.Genre}"));
                    document.Add(new Paragraph($"Pre: You Need To Buy The Book First!!!"));

                    // Close the document to finalize the PDF
                    document.Close();
                }
            }

            // Convert to byte array before the stream is disposed
            byte[] pdfBytes = memoryStream.ToArray();
        
            // Return the PDF bytes as a file
            return File(pdfBytes, "application/pdf", $"{book.Title}.pdf");
        }
    }
    
}