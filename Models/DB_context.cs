using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eLibrary.Models
{
    public class DB_context : DbContext
    {
        public DB_context(DbContextOptions<DB_context> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly configure the Email as the primary key
            modelBuilder.Entity<User>()
                .HasKey(u => u.Email);
            
            modelBuilder.Entity<Book>()
                .HasKey(b => b.isbnNumber);
        }

        // Retrieve all books async
        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await Books.ToListAsync();
        }
        
        // Retrieve all books sync
        public List<Book> GetAllBooks()
        {
            return Books.ToList();
        }
        
        // Retrieve a book by ISBN
        public async Task<Book> GetBookByIsbnAsync(string isbn)
        {
            return await Books.FirstOrDefaultAsync(b => b.isbnNumber == isbn);
        }

        // Add a new book
        public async Task AddBookAsync(Book book)
        {
            await Books.AddAsync(book);
            await SaveChangesAsync();
        }

        // Update an existing book
        public async Task UpdateBookAsync(Book book)
        {
            Books.Update(book);
            await SaveChangesAsync();
        }

        // Delete a book by ISBN
        public async Task DeleteBookByIsbnAsync(string isbn)
        {
            var book = await GetBookByIsbnAsync(isbn);
            if (book != null)
            {
                Books.Remove(book);
                await SaveChangesAsync();
            }
        }
        public async Task<List<User>> GetAllUsersAsync()        // Retrieve all users
        {
            return await Users.ToListAsync();
        }
        public async Task<User> GetUserByEmailAsync(string userEmail)       // Retrieve a user by ID
        {
            return await Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        // Add a new user
        public async Task AddUserAsync(User user)
        {
            await Users.AddAsync(user);
            await SaveChangesAsync();
        }

        // Update an existing user
        public async Task UpdateUserAsync(User user)
        {
            Users.Update(user);
            await SaveChangesAsync();
        }

        // Delete a user by ID
        public async Task DeleteUserByIdAsync(string userEmail)
        {
            var user = await GetUserByEmailAsync(userEmail);
            if (user != null)
            {
                Users.Remove(user);
                await SaveChangesAsync();
            }
        }
    }
}
