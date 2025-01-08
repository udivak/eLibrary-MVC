using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eLibrary.Models;
using eLibrary.Services;
using Microsoft.EntityFrameworkCore;

public class WaitlistService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

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
                var books = await dbContext.Books.ToListAsync();

                // Limit concurrency with SemaphoreSlim
                var semaphore = new SemaphoreSlim(10); // Allow up to 10 concurrent tasks
                var tasks = books.Select(async book =>
                {
                    await semaphore.WaitAsync(stoppingToken);
                    try
                    {
                        await ProcessBookAsync(book, stoppingToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

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

                // Check if the book is back in stock
                var isBookAvailable = await dbContext.UserBook
                    .CountAsync(ub => ub.BookISBN == book.ISBN && !ub.IsPurchased) < 3;

                if (!isBookAvailable)
                    return;

                // Get waiting list for the book
                var waitingList = await dbContext.WaitingLists
                    .Where(w => w.BookISBN == book.ISBN && w.Status == "Pending")
                    .OrderBy(w => w.AddedDate)
                    .ToListAsync();

                foreach (var entry in waitingList)
                {
                    await emailService.SendEmailAsync(entry.UserEmail, "The book is available for purchase!",
                        $"<h1>Hello,</h1>" +
                        $"<p>The book <b>{book.Title}</b> (ISBN: {book.ISBN}) is now available for purchase.</p>" +
                        $"<p>You have 30 minutes to purchase it. If you don't purchase it, it will be offered to the next user in line.</p>");

                    entry.Status = "Notified";
                    dbContext.WaitingLists.Update(entry);
                    await dbContext.SaveChangesAsync();

                    // Wait for the user to respond
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);

                    var userPurchased = await dbContext.UserBook.AnyAsync(ub =>
                        ub.UserEmail == entry.UserEmail && ub.BookISBN == book.ISBN && ub.IsPurchased);

                    if (!userPurchased)
                    {
                        entry.Status = "Skipped";
                        dbContext.WaitingLists.Update(entry);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        break; // Exit if the book was purchased
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
