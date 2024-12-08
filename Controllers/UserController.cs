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
        // Set the creation date and other default properties
        newUser.CreatedAt = DateTime.Today.ToString("d");
        ModelState.Remove("CreatedAt");
    
        // Hash the password before saving to database
        newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
    
        // Set default IsAdmin value
        newUser.IsAdmin = 0;

        if (ModelState.IsValid)
        {
            // Add the new user to the database
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            // Redirect to RegistrationSuccessful page with email
            return RedirectToAction("RegistrationSuccessful", "User", new { email = newUser.Email });
        }

        // If model state is invalid, return the registration view
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
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
    
        // If user not found or password is incorrect
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            // Handle invalid login attempt (e.g., show an error message)
            ModelState.AddModelError("", "Invalid login attempt.");
            return View("UserLogin");
        }

        // If user is found and password is correct, log the user in
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