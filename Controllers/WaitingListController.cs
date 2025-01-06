using eLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace eLibrary.Controllers
{
    public class WaitlistController : Controller
    {
        private readonly DB_context _dbContext;

        public WaitlistController(DB_context dbContext)
        {
            _dbContext = dbContext;
        }

        // Show all books that have waiting lists
        public async Task<IActionResult> Index()
        {
            var books = await _dbContext.Books
                .Where(b => _dbContext.WaitingLists.Any(w => w.BookISBN == b.ISBN))
                .ToListAsync();

            return View("Index", books);
        }

        // Show the waiting list for a specific book
        public async Task<IActionResult> Waitlist(string bookISBN)
        {
            var book = await _dbContext.Books
                .FirstOrDefaultAsync(b => b.ISBN == bookISBN);

            if (book == null)
            {
                return NotFound();
            }

            var waitingList = await _dbContext.WaitingLists
                .Where(w => w.BookISBN == bookISBN)
                .OrderBy(w => w.Position)
                .ToListAsync();

            var model = new WaitListViewModel
            {
                Book = book,
                WaitingList = waitingList
            };

            return View(model);
        }
        
        
        [HttpPost]
        public IActionResult UpdateUserPosition(string bookISBN, string userEmail, int newPosition, int oldPosition)
        {
            if (string.IsNullOrEmpty(bookISBN) || string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Invalid parameters" });
            }

            var waitlistEntry = _dbContext.WaitingLists
                .FirstOrDefault(w => w.UserEmail == userEmail && w.BookISBN == bookISBN);

            if (waitlistEntry != null)
            {
                // Adjust the positions of other entries if needed, e.g., by shifting them
                if (newPosition < oldPosition)
                {
                    // Moving up - Decrease the position of all affected entries
                    var higherEntries = _dbContext.WaitingLists
                        .Where(w => w.Position >= newPosition && w.Position < oldPosition && w.BookISBN == bookISBN)
                        .ToList();
            
                    foreach (var entry in higherEntries)
                    {
                        entry.Position++;
                    }
                }
                else if (newPosition > oldPosition)
                {
                    // Moving down - Increase the position of all affected entries
                    var lowerEntries = _dbContext.WaitingLists
                        .Where(w => w.Position <= newPosition && w.Position > oldPosition && w.BookISBN == bookISBN)
                        .ToList();
            
                    foreach (var entry in lowerEntries)
                    {
                        entry.Position--;
                    }
                }

                // Update the moved entry's position
                waitlistEntry.Position = newPosition;
                _dbContext.SaveChanges();
                return RedirectToAction("Waitlist", new { bookISBN });

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Waitlist entry not found" });
        }

        // Move user up in the list
        [HttpPost]
        public async Task<IActionResult> MoveUserUp(string bookISBN, string userEmail)
        {
            var entry = await _dbContext.WaitingLists
                .FirstOrDefaultAsync(w => w.BookISBN == bookISBN && w.UserEmail == userEmail);

            if (entry != null)
            {
                var previousEntry = await _dbContext.WaitingLists
                    .Where(w => w.BookISBN == entry.BookISBN)
                    .Where(w => w.AddedDate < entry.AddedDate)
                    .OrderByDescending(w => w.AddedDate)
                    .FirstOrDefaultAsync();

                if (previousEntry != null)
                {
                    var tempDate = entry.AddedDate;
                    entry.AddedDate = previousEntry.AddedDate;
                    previousEntry.AddedDate = tempDate;

                    _dbContext.Update(entry);
                    _dbContext.Update(previousEntry);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("WaitList", new { bookISBN });
        }

        // Move user down in the list
        [HttpPost]
        public async Task<IActionResult> MoveUserDown(string bookISBN, string userEmail)
        {
            var entry = await _dbContext.WaitingLists
                .FirstOrDefaultAsync(w => w.BookISBN == bookISBN && w.UserEmail == userEmail);

            if (entry != null)
            {
                var nextEntry = await _dbContext.WaitingLists
                    .Where(w => w.BookISBN == entry.BookISBN)
                    .Where(w => w.AddedDate > entry.AddedDate)
                    .OrderBy(w => w.AddedDate)
                    .FirstOrDefaultAsync();

                if (nextEntry != null)
                {
                    var tempDate = entry.AddedDate;
                    entry.AddedDate = nextEntry.AddedDate;
                    nextEntry.AddedDate = tempDate;

                    _dbContext.Update(entry);
                    _dbContext.Update(nextEntry);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("WaitList", new { bookISBN });
        }

        // Delete user from the waiting list
        [HttpPost]
        public async Task<IActionResult> DeleteUserFromWaitlist(string bookISBN, string userEmail)
        {
            var entry = await _dbContext.WaitingLists
                .FirstOrDefaultAsync(w => w.BookISBN == bookISBN && w.UserEmail == userEmail);

            if (entry != null)
            {
                _dbContext.WaitingLists.Remove(entry);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("WaitList", new { bookISBN });
        }
    }
}
