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
    
    public async Task<IActionResult> Index()
    {
        var books = await _dbContext.GetAllBooksAsync();
        return View(books);
    }
    
    // GET
    public IActionResult BookDetails(Book book)
    {
        return View("BookDetails", book);
    }
    public IActionResult AddBook()
    {
        return View("AddBook", new Book());
    }
    [HttpPost]
    public IActionResult AddBookSubmit(Book book)
    {
        if (ModelState.IsValid)
        {
            // Add the new book to the database
            _dbContext.Books.Add(book);
            _dbContext.SaveChangesAsync();
            TempData["newBook"] = JsonConvert.SerializeObject(book);


            // Redirect to a different page or show a success message
            return RedirectToAction("BookAdded", new { title = book.Title });
        }

        return View("AddBook", book);

        // If model is not valid, re-display the form with validation errors
    }
    public IActionResult DeleteBook(/*Book deletedBook*/)
    {
        Book deletedBook = JsonConvert.DeserializeObject<Book>(TempData["newBook"].ToString());
        return View("DeleteBook", deletedBook);
    }

    public IActionResult BookAdded(string title)
    {
        // Retrieve the added book from TempData
        var addedBook = _dbContext.Books.FirstOrDefault(b => b.Title == title);

        if (addedBook == null)
        {
            // If the book is not found in TempData, redirect to an appropriate page
            return RedirectToAction("Index");
        }

        // Return the BookAdded view with the added book
        return View("BookAdded", addedBook);
    }
    
}