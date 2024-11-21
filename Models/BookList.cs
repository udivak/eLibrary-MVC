namespace eLibrary.Models;

public class BookList
{
    private List<Book> books;
    private string search; //to know the relation of the collection

    public BookList()
    {
        this.books = new List<Book>();
    }
    public BookList(List<Book> books, string search = null)
    {
        this.books = books;
        this.search = search;
    }
    public List<Book> Books => this.books;
    public string Search => this.search;
    
    
    public static BookList GenerateBooks()
    {
        var books = new List<Book>
        {
            new Book("The Great Gatsby", "F. Scott Fitzgerald", "9780743273565", "Scribner", 1925, 10, 2, 16, "Hardcover"),
            new Book("1984", "George Orwell", "9780451524935", "Secker & Warburg", 1949, 8, 1, 18, "Paperback"),
            new Book("To Kill a Mockingbird", "Harper Lee", "9780060935467", "J.B. Lippincott & Co.", 1960, 12, 3, 14, "Hardcover"),
            new Book("The Catcher in the Rye", "J.D. Salinger", "9780316769488", "Little, Brown and Company", 1951, 15, 4, 18, "Paperback"),
            new Book("Moby-Dick", "Herman Melville", "9781851244422", "Richard Bentley", 1851, 18, 5, 16, "Hardcover"),
            new Book("Pride and Prejudice", "Jane Austen", "9780141439518", "T. Egerton", 1813, 10, 2, 12, "Paperback"),
            new Book("The Hobbit", "J.R.R. Tolkien", "9780261102217", "George Allen & Unwin", 1937, 20, 6, 10, "Hardcover"),
            new Book("War and Peace", "Leo Tolstoy", "9781400079988", "The Russian Messenger", 1869, 25, 7, 18, "Paperback"),
            new Book("The Odyssey", "Homer", "9780140268867", "Penguin Classics", 800, 18, 5, 12, "Paperback"),
            new Book("Crime and Punishment", "Fyodor Dostoevsky", "9780140449136", "The Russian Messenger", 1866, 22, 6, 18, "Hardcover"),
            new Book("The Divine Comedy", "Dante Alighieri", "9780142437223", "Mandragora", 1320, 17, 4, 18, "Paperback"),
            new Book("Jane Eyre", "Charlotte Brontë", "9780141441146", "Smith, Elder & Co.", 1847, 16, 3, 14, "Hardcover"),
            new Book("The Brothers Karamazov", "Fyodor Dostoevsky", "9780374528379", "The Russian Messenger", 1880, 20, 6, 18, "Paperback"),
            new Book("Brave New World", "Aldous Huxley", "9780060850524", "Chatto & Windus", 1932, 10, 2, 16, "Hardcover"),
            new Book("Catch-22", "Joseph Heller", "9781451626650", "Simon & Schuster", 1961, 14, 4, 18, "Paperback"),
            new Book("Fahrenheit 451", "Ray Bradbury", "9781451673319", "Ballantine Books", 1953, 11, 3, 16, "Hardcover"),
            new Book("The Grapes of Wrath", "John Steinbeck", "9780143039433", "Viking Press", 1939, 12, 4, 18, "Paperback"),
            new Book("The Road", "Cormac McCarthy", "9780307387899", "Alfred A. Knopf", 2006, 14, 3, 16, "Hardcover"),
            new Book("The Picture of Dorian Gray", "Oscar Wilde", "9780141439570", "Lippincott's Monthly Magazine", 1890, 18, 5, 16, "Paperback"),
            new Book("Les Misérables", "Victor Hugo", "9780451419439", "A. Lacroix, Verboeckhoven & Cie", 1862, 22, 6, 18, "Hardcover")
        };

        return new BookList(books);
    }


}