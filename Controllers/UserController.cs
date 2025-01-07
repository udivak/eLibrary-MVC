using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using eLibrary.Services;


namespace eLibrary.Controllers;
public class UserController : Controller
{
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;
    private readonly IEmailService _emailService;

    public UserController(DB_context dbContext, IHttpContextAccessor context, IEmailService emailService)
    {
        _emailService = emailService;
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
            try
            {
                await _emailService.SendEmailAsync(
                    newUser.Email,
                    "Registration Email",
                    "<h1>Hello</h1><p>Thank you for registering with us. You can now log in to access your account. Have a nice day :W</p>"
                );
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
            await _dbContext.SaveChangesAsync();

            // Redirect to RegistrationSuccessful page with email
            return RedirectToAction("RegistrationSuccessful", "User", new { email = newUser.Email });
        }

        // If model state is invalid, return the registration view
        return View("UserRegistration", newUser);
    }
    
    [HttpGet]
    public async Task<IActionResult> CanBorrowMore()
    {
        string userEmail = HttpContext.Session.GetString("userEmail");
        var userBooks = await _dbContext.UserBook.Where(ub => ub.UserEmail == userEmail && !ub.IsPurchased).CountAsync();
        if (userBooks < 3)
        {
            return Json(new { status = "OK" });
        }
        return Json(new { status = "Error", message = "You already have 3 books borrowed." });
    }
    
    public async Task<IActionResult> CheckBookAvailabilityByEmail()
    {
        var booksInStock = new List<string>();
        var userEmail = HttpContext.Session.GetString("userEmail");
        var waitingList = await _dbContext.WaitingLists.Where(w => w.UserEmail == userEmail).ToListAsync();
        if (waitingList != null)
        {
            // Check if any books are in stock
            foreach (var item in waitingList){
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == item.BookISBN);
            if (book != null)
            {
                booksInStock.Add(book.Title);
                try
                {
                    await _emailService.SendEmailAsync(
                        item.UserEmail,
                        "Book Availability",
                        $"<h1>Hello</h1><p>The book {book.Title} with ISBN {item.BookISBN} is now available. You were waiting for it.</p>"
                    );
                    Console.WriteLine("Email sent to " + item.UserEmail);
                    _dbContext.WaitingLists.Remove(item);
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to {item.UserEmail}: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "Error sending email." });
                }
            }

            // Return the book title as JSON
            // return Ok(new { success = true, booksInStock = new[] { book.Title } });
            }
            return Ok(new { success = true, booksInStock = booksInStock.ToArray() });
        }

        // If no books are in stock, return an empty list
        return Ok(new { success = true, booksInStock = new string[] { } });
    }

    
    
    public async Task<IActionResult> CheckBookAvailability()
    {
        var waitingList = await _dbContext.WaitingLists.ToListAsync();
        
        foreach (var waiting in waitingList)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == waiting.BookISBN);
            if (book != null )
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        waiting.UserEmail,
                        "Book Availability",
                        $"<h1>Hello</h1><p>The book {book.Title} with ISBN {waiting.BookISBN} is now available. You were waiting for it.</p>"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to {waiting.UserEmail}: {ex.Message}");
                }
                
                // Remove the waiting list entry after sending the email
                _dbContext.WaitingLists.Remove(waiting);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        return Ok();
    }
    
    
    public async Task<IActionResult> DeleteFromMyList(string bookISBN)
    {
        string userEmail = HttpContext.Session.GetString("userEmail");

        var userBook = await _dbContext.UserBook
            .FirstOrDefaultAsync(ub => ub.UserEmail == userEmail && ub.BookISBN == bookISBN);
        if (userBook != null)
        {
            _dbContext.UserBook.Remove(userBook);
            await _dbContext.SaveChangesAsync();
        }
        return View("Profile");
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
    public async Task<IActionResult> CheckBookStock()
    {
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { Message = "Email is required." });
        }

        // Current date
        var currentDate = DateTime.Now.Date;

        // Query UserBook for books that are borrowed and expire in 5 days
        var booksEndingInFiveDays = _dbContext.UserBook
            .Join(
                _dbContext.Books,
                ub => ub.BookISBN,
                b => b.ISBN,
                (ub, b) => new { UserBook = ub, Book = b }
            )
            .Where(joined => joined.UserBook.UserEmail == email &&
                             !joined.UserBook.IsPurchased &&
                             joined.UserBook.BorrowExpiryDate.Value == currentDate.AddDays(5))
            .Select(joined => new
            {
                Title = joined.Book.Title,
                Isbn = joined.Book.ISBN,
                BorrowDate = joined.UserBook.BorrowDate,
                BorrowExpiryDate = joined.UserBook.BorrowExpiryDate
            })
            .ToList();

        if (booksEndingInFiveDays.Count == 0)
        {
            return Ok(new { Message = "No books are nearing the end of the borrowing period in 5 days." });
        }

        // Prepare email content
        var emailBody = "The following books you borrowed are due in 5 days:\n\n";
        foreach (var book in booksEndingInFiveDays)
        {
            emailBody += $"- {book.Title} (ISBN: {book.Isbn}), Borrowed On: {book.BorrowDate:yyyy-MM-dd}, Due Date: {book.BorrowExpiryDate:yyyy-MM-dd}\n";
        }

        emailBody += "\nPlease make sure to return or renew them on time.";

        // Send email notification
        try
        {
            await _emailService.SendEmailAsync(
                email,
                "Books Due in 5 Days",
                emailBody
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to send email notification.", Error = ex.Message });
        }

        // Return success response
        return Ok(new { Message = "Email notification sent successfully.", Books = booksEndingInFiveDays });
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

        try
        {
            // Join WaitingList with Books to get the book details for the user's waiting list
            var waitingList = await _dbContext.WaitingLists
                .Where(w => w.UserEmail == userEmail && !string.IsNullOrEmpty(w.BookISBN))
                .Join(
                    _dbContext.Books,
                    w => w.BookISBN,
                    b => b.ISBN,
                    (w, b) => new WaitingListViewModel
                    {
                        BookISBN = w.BookISBN,
                        BookTitle = b.Title,
                        QuantityRequested = w.QuantityRequested,
                        Status = w.Status,
                        AddedDate = w.AddedDate
                    })
                .ToListAsync();

            // Check if the waiting list is empty
            if (!waitingList.Any())
            {
                // Log or debug to check if the waiting list is empty
                Console.WriteLine($"Waiting list is empty for user: {userEmail}");
                return PartialView("_MyList", new List<WaitingListViewModel>());
            }

            // Return the books in the waiting list as a partial view
            return PartialView("_MyList", waitingList);
        }
        catch (Exception ex)
        {
            // Log the error (could be improved with more sophisticated logging)
            Console.WriteLine($"Error retrieving books for user {userEmail}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
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
                b => b.ISBN,
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