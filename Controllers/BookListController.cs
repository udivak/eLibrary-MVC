using System.Collections.Generic;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Controllers;

public class BookListController : Controller
{
    public IActionResult AllBooks(int page = 1)
    {
        BookList books = BookList.GenerateBooks();
        //get from the db all the Books
        
        return View("BooksList", books);
    }

    [Route("BookList/{category}")]
    public IActionResult Category(string category)
    {
        
        if (string.IsNullOrEmpty(category))
        {
            return RedirectToAction("AllBooks");
        }

        List<Book> books = new List<Book>();
        BookList testBooks = BookList.GenerateBooks();

        return View("BooksList", testBooks);
    }
}
