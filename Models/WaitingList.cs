using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Models;

[Table("WaitingList")]
public class WaitingList
{
    [Key]
    [Column(Order = 0)]
    public string BookISBN { get; set; } // Foreign key to the Books table

    [Key]
    [Column(Order = 1)]
    public string UserEmail { get; set; } // Foreign key to the Users table

    [Required]
    public int QuantityRequested { get; set; }

    [Required]
    public DateTime AddedDate { get; set; } = DateTime.Now;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";
        
        
    public WaitingList(string bookISBN, string userEmail, int quantityRequested)
    {
        BookISBN = bookISBN;
        UserEmail = userEmail;
        QuantityRequested = quantityRequested;
    }
}