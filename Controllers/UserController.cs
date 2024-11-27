using System;
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
    public IActionResult RegistrationSubmit(User newUser)
    {
        newUser.CreatedAt = DateTime.Today.ToShortDateString();
        newUser.IsAdmin = false;

        return View("UserRegistration", newUser);
    }
    public IActionResult LoginPage()
    {
        return View("UserLogin");
    }
    public IActionResult Login(string UserName, string Password)
    {
        //Assume that verify user details for now;
        //check if the user is in the db
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