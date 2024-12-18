using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eLibrary.Controllers;
public class UserController : Controller
{
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;
    public UserController(DB_context dbContext, IHttpContextAccessor context)
    {
        _dbContext = dbContext;
        _context = context;
    }
    private ISession Session => _context.HttpContext.Session;
    
    public IActionResult Logout()
    {
        Session.Clear(); // This clears all session data
        return RedirectToAction("Index", "Home"); // Redirects to home page after logout
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

    public IActionResult LoginPage()            //old
    {
        return View("UserLogin");
    }

    public IActionResult Login(string email, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))     //Login failed
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            TempData["LoginMessage"] = "The Password is incorrect. Please try again.";
            return RedirectToAction("Index", "Home");
        }
        // init all Session vars for user
        Session.SetString("userName", user.UserName);
        Session.SetInt32("isAdmin", user.IsAdmin);
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

    /*public IActionResult Checkout()
    {
        //Testing with current data
        List<Book> books = _dbContext.GetAllBooks().Take(4).ToList();
        List<CartItem> cart = new List<CartItem>();
        Random random = new Random();
        foreach (var book in books)
        {
            CartItem temp = new CartItem(book.isbnNumber, book.Title, "Buy", book.BuyPrice, random.Next(1, 6));
            cart.Add(temp);
        }

        return View("Checkout", cart);
    }*/
    



    public IActionResult Profile()
    {
        List<Book> featuredBooks = _dbContext.GetAllBooks().Take(8).ToList();
        return View();
    }
    public IActionResult MyList()
    {
        // Logic to retrieve the user's list of books (this could be from a database)
        // var userList = _dbContext.GetUserList(); // Replace with actual service call
        return PartialView("_MyList");
        
        return View();
    }

    // Action to display the user's purchased books
    public IActionResult MyBooks()
    {
        // Logic to retrieve the user's purchased books
        // var myBooks = _dbContext.GetMyBooks(); // Replace with actual service call
        return PartialView("_MyBooks");
        // return View();
    }
    public IActionResult PersonalDetails()
    {
        // Fetch the user's personal details
        // var personalDetails = GetUserPersonalDetails();
        return PartialView("_PersonalDetails");
    }
    private IEnumerable<Book> GetUserList()
    {
        // Replace with actual logic to fetch user's list
        return new List<Book> { new Book { Title = "Book 1" }, new Book { Title = "Book 2" } };
    }

    private IEnumerable<Book> GetUserBooks()
    {
        // Replace with actual logic to fetch user's books
        return new List<Book> { new Book { Title = "My Book 1" }, new Book { Title = "My Book 2" } };
    }

}