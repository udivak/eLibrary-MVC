using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace eLibrary.Models;

public class DBConnection : DbContext
{
    public DBConnection(DbContextOptions<DBConnection> options) : base(options)
    { }
    
    public DbSet<Book> Books { get; set; }
    
}