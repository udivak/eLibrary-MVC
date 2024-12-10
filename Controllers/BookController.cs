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

    public BookController(DB_context dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IActionResult> GetFirstBook()
    {
        var books = await _dbContext.GetAllBooksAsync();
        Book temp = books.FirstOrDefault(); //get first book from db 
        
        return View("BookDetails",temp);
    }
    public IActionResult BookDetailsTest()
    {
        Book test = new Book("title", "author", "ISBN", "publisher", 2024, 200, 100, 6, "pdf", "novel");
        return View("BookDetails", test);
    }
    public async Task<IActionResult> BookDetails(string isbn)
    {
        var book = _dbContext.Books.FirstOrDefault(b => b.isbnNumber == isbn);

        if (book == null)
        {
            return NotFound(); // Return 404 if the book is not found
        }

        return View(book); // Pass the book to the view
    }

    public IActionResult AddBook()
    {
        return View("AddBook", new Book());
    }
    [HttpPost]
    public async Task<IActionResult> AddBookSubmit(Book book)
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
    
    
    public IActionResult Searchbooks(string title, string author, string genre, string publisher, int? year, string format)
    {
        // Start with an empty list of books
        var books = new List<eLibrary.Models.Book>();

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

    public IActionResult SearchResults()
    {
        return View(_dbContext.Books.ToList());
    }
    
}