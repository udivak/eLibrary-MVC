using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

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
    
    [HttpPost]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        // Validate the passwords
        if (newPassword != confirmNewPassword)
        {
            ModelState.AddModelError("", "New passwords do not match.");
            return View(); // Return the same view with the error
        }

        // Get the user from the database by the username or user ID
        var username = User.Identity.Name; // Assuming you use the username to identify the user
        var user = _dbContext.Users.FirstOrDefault(u => u.UserName == username); 

        if (user == null)
        {
            return NotFound(); // If user not found
        }

        // Verify the current password with the stored hashed password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password)) // Assuming password is hashed
        {
            ModelState.AddModelError("", "Current password is incorrect.");
            return View(); // Return the same view with the error
        }

        // Hash the new password
        var hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

        // Update the password in the database
        user.Password = hashedNewPassword;
        _dbContext.SaveChanges(); // Save changes to the database

        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction("Profile"); // Redirect to the profile or success page
    }
    
   
    [HttpGet]
    public IActionResult CheckBookStock()
    {
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { Message = "Email is required." });
        }

        var booksInStock = _dbContext.WaitingLists
            .Join(
                _dbContext.Books,
                wl => wl.BookISBN,
                b => b.isbnNumber,
                (wl, b) => new { WaitingList = wl, Book = b }
            )
            .Where(joined => joined.WaitingList.UserEmail == email && 
                             joined.Book.Quantity >= joined.WaitingList.QuantityRequested)
            .Select(joined => new
            {
                Title = joined.Book.Title,
                Isbn = joined.Book.isbnNumber,
                QuantityRequested = joined.WaitingList.QuantityRequested,
                QuantityAvailable = joined.Book.Quantity
            })
            .ToList();

        // Return the result
        if (booksInStock.Count == 0)
        {
            return NotFound(new { Message = "No books are in stock for this user." });
        }

        return Ok(booksInStock);
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
        Session.SetString("userEmail", user.Email);
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
        return View("Profile");
    }
    public async Task<IActionResult> MyList()
    {
    // Get the user's email from the session
    string userEmail = HttpContext.Session.GetString("userEmail");

    if (string.IsNullOrEmpty(userEmail))
    {
        return Unauthorized("User is not logged in.");
    }

    // Retrieve all ISBNs from the WaitingList for the current user
    var waitingList = await _dbContext.WaitingLists
        .Where(w => w.UserEmail == userEmail && !string.IsNullOrEmpty(w.BookISBN))
        .ToListAsync();

    // Check if the waiting list is empty
    if (!waitingList.Any())
    {
        // Log or debug to check if the waiting list is empty
        Console.WriteLine("Waiting list is empty for user: " + userEmail);
        return PartialView("_MyList", new List<Book>());
    }

    // Get the ISBN numbers of the books in the waiting list
    var isbnNumbers = waitingList.Select(w => w.BookISBN).ToList();

    // Retrieve the books from the Books table using the ISBN numbers
    var booksInWaitingList = await _dbContext.Books
        .Where(b => isbnNumbers.Contains(b.isbnNumber))
        .ToListAsync();

    // Return the books as a partial view
    return PartialView("_MyList", booksInWaitingList);
}


    // Action to display the user's purchased books
    public async Task<IActionResult> MyBooks()
    {
        // Get the user's email from the session
        string userEmail = HttpContext.Session.GetString("userEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized("User is not logged in.");
        }

        // Join UserBook with Books to include the title
        var userBooks = await _dbContext.UserBook
            .Where(ub => ub.UserEmail == userEmail)
            .Join(
                _dbContext.Books,
                ub => ub.BookISBN,
                b => b.isbnNumber,
                (ub, b) => new UserBookView
                {
                    UserBookId = ub.Id,
                    UserEmail = ub.UserEmail,
                    BookISBN = ub.BookISBN,
                    BookTitle = b.Title,
                    PurchaseDate = ub.PurchaseDate,
                    BorrowDate = ub.BorrowDate,
                    BorrowExpiryDate = ub.BorrowExpiryDate,
                    IsPurchased = ub.IsPurchased
                })
            .ToListAsync();

        // Check if the user has any books
        if (!userBooks.Any())
        {
            Console.WriteLine("No books found for user: " + userEmail);
        }

        // Return the user books with titles as a partial view
        return PartialView("_MyBooks", userBooks);
    }

    
    public IActionResult PersonalDetails()
    {
        var user = _dbContext.Users.FirstOrDefault(user => user.Email == Session.GetString("userEmail"));
        return PartialView("_PersonalDetails", user);
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

    public async Task<IActionResult> ManageUsers()
    {
        List<User> allUsers = await _dbContext.GetAllUsersAsync();
        return View("ManageUsers", allUsers);
    }

    public async Task<IActionResult> RemoveUser(string email)
    {
        var deleteUser = await _dbContext.GetUserByEmailAsync(email);
        if (deleteUser == null)
        {
            return View("Error");
        }
        _dbContext.Users.Remove(deleteUser);
        _dbContext.SaveChangesAsync();
        return RedirectToAction("ManageUsers");
    }
    

}