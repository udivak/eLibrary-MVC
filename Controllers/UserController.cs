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
        Session.Clear();
        return RedirectToAction("Index", "Home");
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
    
        newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
    
        newUser.IsAdmin = 0;

        if (ModelState.IsValid)
        {
            await _dbContext.Users.AddAsync(newUser);
            try
            {
                await _emailService.SendEmailAsync(
                    newUser.Email,
                    "Registration Email",
                    "<h1>Hello</h1><p>Thank you for registering iReadit. You can now log in to access your account.</p>"
                );
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("RegistrationSuccessful", "User", new { email = newUser.Email });
        }
        // If model state is invalid, return the registration view
        return View("UserRegistration", newUser);
    }
    
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> CanBorrowMore(string bookISBN)
    {
        string userEmail = Session.GetString("userEmail");
        var userBooks = await _dbContext.UserBook.Where(ub => ub.UserEmail == userEmail && !ub.IsPurchased).CountAsync();
        var borrowed_in_Cart = ShoppingCart.GetBorrowdBookCount();
        if (userBooks + borrowed_in_Cart < 3)
        {
            return Ok(new { status = "OK" });
        }
        else
        {
            // Add the book to the waiting list
            // var waitingListItem = new WaitingList(bookISBN, userEmail, 1);
            // await _dbContext.WaitingLists.AddAsync(waitingListItem);
            // await _dbContext.SaveChangesAsync();
            return BadRequest(new { status = "Error", message = "You already have 3 books borrowed. The book was added to your waiting list." });
        }
    }
    
    public async Task<IActionResult> CheckBookAvailabilityByEmail()
    {
        await CheckBookStock();

        var booksInStock = new List<string>();
        var userEmail = HttpContext.Session.GetString("userEmail");
        var waitingList = await _dbContext.WaitingLists.Where(w => w.UserEmail == userEmail).ToListAsync();
        if (waitingList != null)
        {
            // Check if any books are in stock
            foreach (var item in waitingList)
            {
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
            if (book != null)
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
        return RedirectToAction("Profile");
    }
    
    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (newPassword != confirmNewPassword)
        {
            TempData["ChangePasswordMsg"] = "New passwords do not match. please try again.";
            return View("Profile");
        }

        var userEmail = Session.GetString("userEmail");
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == userEmail); 
        if (user == null)
        {
            return NotFound();
        }
        
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
        {
            TempData["ChangePasswordMsg"] = "Current password is incorrect. please try again.";
            return RedirectToAction("Profile");
        }
        
        var hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.Password = hashedNewPassword;
        await _dbContext.SaveChangesAsync();

        TempData["ChangePasswordMsg"] = "Password changed successfully.";
        return RedirectToAction("Profile");
    }
    
    [HttpGet]
    public async Task<IActionResult> CheckBookStock()
    {
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { Message = "Email is required." });
        }

        var currentDate = DateTime.Now;

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
                             joined.UserBook.BorrowExpiryDate.Value <= currentDate.AddDays(5))
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
    
    public async Task<IActionResult> RegistrationSuccessful(string email)
    {
        var userAdded = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (userAdded == null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View("UserAdded", userAdded);
    }

    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
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
    
    public IActionResult Profile()
    {
        return View("Profile");
    }
    
    public async Task<IActionResult> MyList()
    {
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

            if (!waitingList.Any())
            {
                Console.WriteLine($"Waiting list is empty for user: {userEmail}");
                return PartialView("_MyList", new List<WaitingListViewModel>());
            }

            return PartialView("_MyList", waitingList);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving books for user {userEmail}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
    
    public async Task<IActionResult> MyBooks()
    {
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
    
    public async Task<IActionResult> PersonalDetails()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == Session.GetString("userEmail"));
        return PartialView("_PersonalDetails", user);
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
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("ManageUsers");
    }
}