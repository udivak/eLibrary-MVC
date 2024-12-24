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
        var book = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);
        //var book = _dbContext.GetAllBooks().First();
        if (book == null)
        {
            return NotFound(); // Return 404 if the book is not found
        }

        return View("BookDetails", book); // Pass the book to the view
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
            return RedirectToAction("BookAdded", new { isbn = book.isbnNumber });   //Success Message
        }
        return View("AddBook", book);
    }

    public IActionResult DeleteBook(Book deletedBook)       //Delete new book without adding to DB
    {
        return View("DeleteBook", deletedBook);
    }
    
    public IActionResult FindABook(string title, string author, string genre, string publisher, int? year, string format)
    {
        // Start with an empty list of books
        var books = new List<Book>();

        // Apply filters based on the user's input
        if (!string.IsNullOrEmpty(title))
        {
            books = _dbContext.Books.Where(b => b.Title.Contains(title)).ToList();
        }
        else if (!string.IsNullOrEmpty(author))
        {
            books = _dbContext.Books.Where(b => b.Author.Contains(author)).ToList();
        }
        else if (!string.IsNullOrEmpty(genre) && Enum.TryParse<Genre>(genre, true, out var genreEnum))
        {
            books = _dbContext.Books.Where(b => b.Genre == genreEnum).ToList();
        }
        else if (!string.IsNullOrEmpty(publisher))
        {
            books = _dbContext.Books.Where(b => b.Publisher.Contains(publisher)).ToList();
        }
        else if (year.HasValue)
        {
            books = _dbContext.Books.Where(b => b.Year == year.Value).ToList();
        }
        else if (!string.IsNullOrEmpty(format))
        {
            books = _dbContext.Books.Where(b => b.Format == format).ToList();
        }
        else
        {
            List<Book> featuredBooks = _dbContext.GetAllBooks().Take(8).ToList();
            return View(featuredBooks);
        }
        // Return an empty list if no filters are applied (first time visiting)
        return View("SearchResults", books);
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

        return PartialView("_BookList", books.ToList());
    }

    public IActionResult FilterBooksByCategory(string genre)
    {
        Genre bookGenre = (Genre)Enum.Parse(typeof(Genre), genre, true);
        var books = _dbContext.Books.Where(b => b.Genre == bookGenre);
        return PartialView("_BookList", books.ToList());
    }
    
    [HttpGet]
    public IActionResult BookAdded(string isbn)
    {
        var addedBook = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);
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
        var book = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);
        if (book == null)
            return RedirectToAction("Error", "Home");
        TempData["BookTitle"] = $"{book.Title}";
        if (book.Format == "Physical" && book.Quantity < quantity)                  //No more physical copies
        { 
            TempData["AddToCartMessage"] = "FAIL";
            string currentUser = Session.GetString("userEmail");
            var waitingList = new WaitingList(isbn, currentUser, quantity);
            _dbContext.WaitingLists.Add(waitingList);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        int price;
        if (cartAction == "Buy")
        {
            price = book.BuyPrice;
        }
        else
        {
            price = book.BorrowPrice;
        }
        CartItem addItem = new CartItem(book.isbnNumber, book.Title, cartAction, price, quantity);
        ShoppingCart.Add(addItem);
        TempData["AddToCartMessage"] = "SUCCESS";
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> RemoveFromCart(string isbn)
    {
        var book = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);
        if (book == null)
            return RedirectToAction("Error", "Home");
        TempData["BookTitle"] = $"'{book.Title}'";
        var shoppingCart = ShoppingCart.GetShoppingCart();
        CartItem removedItem = shoppingCart.Find(item => item.ISBN == isbn);
        int qty = removedItem.Quantity;
        if (book.Format == "Physical")
        {
            book.Quantity += qty;
            await _dbContext.SaveChangesAsync();
        }
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
        var book = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);
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
                    document.Add(new Paragraph($"ISBN: {book.isbnNumber}"));
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