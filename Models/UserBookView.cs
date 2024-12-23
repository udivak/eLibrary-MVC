namespace eLibrary.Models;

public class UserBookView
{
        public int UserBookId { get; set; }
        public string UserEmail { get; set; }
        public string BookISBN { get; set; }
        public string BookTitle { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? BorrowExpiryDate { get; set; }
        public bool IsPurchased { get; set; }
}