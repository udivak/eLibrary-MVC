using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;

namespace eLibrary.Controllers;

public class UserController : Controller
{
    // GET
    public IActionResult Registration()
    {
        return View("UserRegistration");
    }
    
    public IActionResult RegistrationSubmit()
    {
        User user = new User();
        user.FirstName = Request.Form["FirstName"];
        user.LastName = Request.Form["LastName"];
        user.UserName = Request.Form["UserName"];
        user.Password = Request.Form["Password"];
        user.Email = Request.Form["Email"];
        //user.CreatedAt = Request.Form["CreatedAt"];
        user.CreatedAt = DateTime.Today.ToShortDateString();
        user.Address = Request.Form["Address"];
        return View("UserDetails", user);
    }
}