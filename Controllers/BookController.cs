using System.Text.Json.Serialization;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace eLibrary.Controllers;
public class BookController : Controller
{
    // GET
    public IActionResult BookDetails() //test func
    {
        Book LOTR = new Book("Lord Of The Rings", "author", "12345", "publisher", 2024,
            100, 25, 18, "pdf", "");
        return View("BookDetails", LOTR);
    }
    public IActionResult AddBook()
    {
        return View("AddBook");
    }
    public IActionResult AddBookSubmit()
    {
        Book newBook = new Book();
        newBook.Title = Request.Form["title"];
        newBook.Author = Request.Form["author"];
        newBook.ISBN = Request.Form["isbnNumber"];
        newBook.Publisher = Request.Form["publisher"];
        newBook.Year = int.Parse(Request.Form["year"]);
        newBook.BuyPrice = int.Parse(Request.Form["buyPrice"]);
        newBook.BorrowPrice = int.Parse(Request.Form["borrowPrice"]);
        newBook.AgeLimit = int.Parse(Request.Form["ageLimit"]);
        newBook.Format = Request.Form["format"];
        // Serialize the book object to JSON
        string jsonBook = JsonConvert.SerializeObject(newBook);
        // Store it in TempData or use it as needed
        TempData["newBook"] = jsonBook;
        return View("BookDetails", newBook);
    }
    public IActionResult DeleteBook()
    {
        Book deletedBook = JsonConvert.DeserializeObject<Book>(TempData["newBook"].ToString());
        return View("DeleteBook", deletedBook);
    }
    
}