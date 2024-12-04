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
            _dbContext.Books.Add(book);     // Add the new book to the database
            _dbContext.SaveChangesAsync();
            return RedirectToAction("BookAdded", new { isbn = book.isbnNumber });   //Success Message
        }
        return View("AddBook", book);
    }
    public IActionResult DeleteBook(Book deletedBook)       //Delete new book without adding to DB
    {
        return View("DeleteBook", deletedBook);
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
    
}