namespace eLibrary.Models;

public class WaitingListViewModel
{
    public string BookISBN { get; set; }
    public string BookTitle { get; set; }
    public int QuantityRequested { get; set; }
    public string Status { get; set; }
    public DateTime AddedDate { get; set; }
}
