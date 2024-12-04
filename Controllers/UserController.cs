using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eLibrary.Controllers;
public class UserController : Controller
{
    private DB_context _dbContext;
    public UserController(DB_context dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IActionResult Registration()
    {
        User user = new User();
        return View("UserRegistration", user);
    }
    [HttpPost]
    public async Task<IActionResult> RegistrationSubmit(User newUser)
    {
        newUser.CreatedAt = DateTime.Today.ToString("d");
        ModelState.Remove("CreatedAt");
        
        newUser.IsAdmin = 0;
        if (ModelState.IsValid)
        {
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("RegistrationSuccessful", "User", new {email = newUser.Email});
        }
        return View("UserRegistration", newUser);
    }
    public IActionResult RegistrationSuccessful(string email)
    {
        var userAdded = _dbContext.Users.FirstOrDefault(u => u.Email == email);
        if (userAdded == null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View("UserAdded", userAdded);
    }
    public IActionResult LoginPage()
    {
        return View("UserLogin");
    }

    public IActionResult Login(string email, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(u => (u.Email == email && u.Password == password));
        if (user == null)
        {
            return RedirectToAction("LoginPage", "User");
        }
        Console.WriteLine("Logged In");
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult LoginTest(string userName, string password)
    {
        //Assume that verify user details for now;
        //check if the user is in the db
        User currentUser = new User
        {
            UserName = userName,
            Address = "nothing",
            Email = "nothing",
            CreatedAt = DateTime.Today.ToShortDateString(),
            IsAdmin = 0,
            Password = password,
            FirstName = "nothing",
            LastName = "nothing"
        };
        if (currentUser.IsAdmin ==0)
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