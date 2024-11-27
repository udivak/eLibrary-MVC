using System.ComponentModel.DataAnnotations;

namespace eLibrary.Models;

public class Book
{
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Title must be be 1-50 characters.")]
    public string Title { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Author name must be be 1-50 characters.")]
    public string Author { get; set; }
    [Required]
    [RegularExpression("^[0-9]{1,13}$", ErrorMessage = "ISBN Number must be 1-13 digits.")]        //a number between 1-13 digits
    public string isbnNumber  { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Publisher name must be be 1-50 characters.")]
    public string Publisher { get; set; }
    [Required]
    [Range(0, 2024, ErrorMessage = "Year must be between 0-2024.")]
    public int Year { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Buy Price must be a positive number.")]
    public int BuyPrice { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Borrow Price must be a positive number.")]
    public int BorrowPrice { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Age Limit must be a positive number.")]
    public int AgeLimit { get; set; }
    [Required]
    [RegularExpression("^(PDF|ePub|f2b|mobi)$")]
    public string Format { get; set; }                        //eBook formats - epub, f2b, mobi, pdf
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be be 1-50 characters.")]
    public string Genre { get; set; }
    public Book()
    {
        //default ctor    
    }
    public Book(string title, string author, string ISBN, string publisher, int year, int buyPrice, int borrowPrice,
        int ageLimit, string format, string genre)
    {
        Title = title;
        Author = author;
        isbnNumber = ISBN;
        Publisher = publisher;
        Year = year;
        BuyPrice = buyPrice;
        BorrowPrice = borrowPrice;
        AgeLimit = ageLimit;
        Format = format;
        Genre = genre;
    }
   /*
    public string Title { get => title; set => title = value; }
    public string Author { get => author; set => author = value; }
    public string ISBN { get => isbnNumber; set => isbnNumber = value; }
    public string Publisher { get => publisher; set => publisher = value; }
    public int Year { get => year; set => year = value; }
    public int BuyPrice { get => buyPrice; set => buyPrice = value; }
    public int BorrowPrice { get => borrowPrice; set => borrowPrice = value; }
    public int AgeLimit { get => ageLimit; set => ageLimit = value; }
    public string Format { get => format; set => format = value; }
    public string Category { get => category; set => category = value; }
    */
}