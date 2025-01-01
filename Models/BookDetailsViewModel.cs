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
    
    public float GetAverageRating()
    {
        if (BookReviews.Count == 0)
        {
            return 0;
        }
        float totalRating = 0;
        foreach (var review in BookReviews)
        {
            totalRating += review.Stars;
        }
        return (float)Math.Round(totalRating / BookReviews.Count, 1); 
    }
    
}