using System;
using System.Text.Json.Serialization;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
    
    public IActionResult SearchBooks(string title, string author, string genre, string publisher, int? year, string format)
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
        else if (!string.IsNullOrEmpty(genre))
        {
            books = _dbContext.Books.Where(b => b.Genre.Contains(genre)).ToList();
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
            return View(books);
        }
        // Return an empty list if no filters are applied (first time visiting)
        return View("SearchResults", books);
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
        isbn = "1111111111111";     //test physical book isbn
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
        if (book.Format == "Physical")
        {
            if (book.Quantity > quantity)      //Save the current copy in the user's cart
            {
                book.Quantity -= quantity;
                await _dbContext.SaveChangesAsync();
            }
            else                        //No more physical copies
            {
                ViewBag.Message = $"Sorry, we don't have enough copies of {book.Title} at the moment.";  //simulate message box
                return RedirectToAction("Index", "Home");
            }
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
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> RemoveFromCart(Book book)
    {
        string isbn = book.isbnNumber;
        var b = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);
        if (b == null)
            return RedirectToAction("Error", "Home");
        var shoppingCart = ShoppingCart.GetShoppingCart();
        CartItem removedItem = shoppingCart.Find(item => item.ISBN == isbn);
        int qty = removedItem.Quantity;
        if (b.Format == "Physical")
        {
            b.Quantity += qty;
            await _dbContext.SaveChangesAsync();
        }
        ShoppingCart.Remove(removedItem);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult SearchResults()
    {
        return View(_dbContext.Books.ToList());
    }
    
}