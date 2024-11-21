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
    
    public IActionResult RegistrationSubmit(User newuser)
    {
        // User user = new User();
        // user.FirstName = Request.Form["FirstName"];
        // user.LastName = Request.Form["LastName"];
        // user.UserName = Request.Form["UserName"];
        // user.Password = Request.Form["Password"];
        // user.Email = Request.Form["Email"];
        //user.CreatedAt = Request.Form["CreatedAt"];
        newuser.CreatedAt = DateTime.Today.ToShortDateString();
        // user.Address = Request.Form["Address"];
        newuser.IsAdmin = false;

        return View("UserDetails", newuser);
    }

    public IActionResult Login(string UserName, string Password)
    {
        //Assume that verify user details for now;
        //cheeck if the user is in the db
        User currentUser = new User
        {
            UserName = UserName,
            Address = "nothing",
            Email = "nothing",
            CreatedAt = DateTime.Today.ToShortDateString(),
            IsAdmin = false,
            Password = Password,
            FirstName = "nothing",
            LastName = "nothing"
        };
        if (currentUser.IsAdmin)
        {
            // Login successful
            //login to admin dashboard
            return RedirectToAction("Dashboard");  // Redirect to a Dashboard 
        }
        else
        {
            // Invalid credentials
            // ViewBag.ErrorMessage = "Invalid username or password.";
            //return dashboard for simple users
            
            return RedirectToAction();
        }
    }
}