using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eLibrary.Controllers;
using eLibrary.Models;
using eLibrary.Services;

public class WaitlistService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly object _dbLock = new object();

    public WaitlistService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DB_context>();
                var books = dbContext.Books.ToList();

                // Process each book in parallel
                var tasks = books.Select(book => Task.Run(() => ProcessBookAsync(book, stoppingToken)));
                await Task.WhenAll(tasks);
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); 
        }
    }

    private async Task ProcessBookAsync(Book book, CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine("Processing book: " + book.Title);
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DB_context>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                bool isBookAvailable;
                lock (_dbLock)
                {
                    // Check if the book is back in stock
                    isBookAvailable = dbContext.UserBook
                        .Count(ub => ub.BookISBN == book.ISBN && !ub.IsPurchased) < 3;
                }

                if (isBookAvailable)
                {
                    List<WaitingList> waitingList;
                    lock (_dbLock)
                    {
                        // Retrieve the waiting list for the current book
                        waitingList = dbContext.WaitingLists
                            .Where(w => w.BookISBN == book.ISBN && w.Status == "Pending")
                            .OrderBy(w => w.AddedDate)
                            .ToList();
                    }

                    foreach (var entry in waitingList)
                    {
                        await emailService.SendEmailAsync(entry.UserEmail, "The book is available for purchase!",
                            $"<h1>Hello,</h1>" +
                            $"<p>The book <b>{book.Title}</b> (ISBN: {book.ISBN}) is now available for purchase.</p>" +
                            $"<p>You have 30 minutes to purchase it. If you don't purchase it, it will be offered to the next user in line.</p>"
                        );

                        lock (_dbLock)
                        {
                            entry.Status = "Notified";
                            dbContext.WaitingLists.Update(entry);
                            dbContext.SaveChanges();
                        }

                        // Wait for the user to respond
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);

                        bool userPurchased;
                        lock (_dbLock)
                        {
                            // Check if the user purchased the book
                            userPurchased = dbContext.UserBook.Any(ub => 
                                ub.UserEmail == entry.UserEmail && ub.BookISBN == book.ISBN && ub.IsPurchased);
                        }

                        if (!userPurchased)
                        {
                            lock (_dbLock)
                            {
                                entry.Status = "Skipped";
                                dbContext.WaitingLists.Update(entry);
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            break; // Exit if the book was purchased
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing book {book.ISBN}: {ex.Message}");
        }
    }
}
