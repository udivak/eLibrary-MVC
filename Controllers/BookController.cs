using System;
using System.Text.Json.Serialization;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace eLibrary.Controllers;
public class BookController : Controller
{
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