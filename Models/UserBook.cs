namespace eLibrary.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserBook
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserEmail { get; set; }

    [Required]
    public int BookId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime? BorrowExpiryDate { get; set; }

    [Required]
    public bool IsPurchased { get; set; }
}
