namespace eLibrary.Models;

public class IndexViewModel
{
    public List<Book> FeaturedBooks;
    public List<eLibraryFeedback> Feedbacks;

    public IndexViewModel()
    {
        FeaturedBooks = new List<Book>();
        Feedbacks = new List<eLibraryFeedback>();
    }

    public IndexViewModel(List<Book> featuredBooks, List<eLibraryFeedback> feedbacks)
    {
        FeaturedBooks = featuredBooks;
        Feedbacks = feedbacks;
    }
    
}