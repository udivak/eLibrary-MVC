using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Models;
[Table("UserBook")]
public class UserBook
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserEmail { get; set; }

    [Required]
    public string BookISBN { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime? BorrowExpiryDate { get; set; }

    [Required]
    public bool IsPurchased { get; set; }
    
    public UserBook() { }

    public UserBook(string userEmail, string isbn, bool isPurchased)
    {
        UserEmail = userEmail;
        BookISBN = isbn;
        IsPurchased = isPurchased;
        if (!isPurchased)                               // false = borrowed
        {
            BorrowDate = DateTime.Now;
            BorrowExpiryDate = DateTime.Now.AddDays(30);
        }
        else
        {
            PurchaseDate = DateTime.Now;
        }
    }
}