using System;
using System.Text.Json.Serialization;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using eLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eLibrary.Controllers;
public class BookController : Controller
{
    private readonly DBConnection _context;
    public BookController(DBConnection context)
    {
        _context = context;
    }
    //TESTING CONNECTION TO DB
    public async Task<IActionResult> GetFirstBook()
    {
        // Retrieve the first book from the Books table
        var book = await _context.Books.FirstOrDefaultAsync();

        if (book != null)
        {
            // Print the first book to console
            Console.WriteLine($"Title: {book.Title}, Author: {book.Author}, ISBN: {book.isbnNumber}, Publisher: {book.Publisher}");
        }
        else
        {
            Console.WriteLine("No books found.");
        }

        return View("BookDetails", book);  // Optionally return the first book to the view
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
    public IActionResult AddBookSubmit(Book newBook)
    {
        if (ModelState.IsValid) {
            string jsonBook = JsonConvert.SerializeObject(newBook); // Serialize the book object to JSON
            // Store it in TempData or use it as needed
           TempData["newBook"] = jsonBook;
            return View("BookDetails", newBook);
        }
        return View("AddBook", newBook);
    }
    public IActionResult DeleteBook(/*Book deletedBook*/)
    {
        Book deletedBook = JsonConvert.DeserializeObject<Book>(TempData["newBook"].ToString());
        return View("DeleteBook", deletedBook);
    }

    public IActionResult BookAdded(/*Book book*/)
    {
        Book addedBook = JsonConvert.DeserializeObject<Book>(TempData["newBook"].ToString());
        return View("BookAdded", addedBook);
    }
    
}