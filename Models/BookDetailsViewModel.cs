namespace eLibrary.Models;

public class BookDetailsViewModel
{
    public Book Book;
    public List<BookReview> BookReviews;

    public BookDetailsViewModel()
    {
        Book = new Book();
        BookReviews = new List<BookReview>();
    }

    public BookDetailsViewModel(Book book, List<BookReview> bookReviews)
    {
        Book = book;
        BookReviews = bookReviews;
    }
    
}