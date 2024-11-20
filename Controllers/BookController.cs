using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Controllers;

public class BookController : Controller
{
    // GET
    public IActionResult BookDetails()
    {
        Book LOTR = new Book("Lord Of The Rings", "author", "12345", "publisher", 2024,
            100, 25, 18, "pdf");
        return View("BookDetails", LOTR);
    }
    
}