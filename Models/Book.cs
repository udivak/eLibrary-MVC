using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Models;

public class Book
{
    private string _title;
    [Required]
    [Column("Title")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Title must be be 1-50 characters.")]
    public string Title 
    { 
        get => _title; 
        set => _title = value;
    }
    
    private string _author;
    [Required]
    [Column("Author")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Author name must be be 1-50 characters.")]
    public string Author
    {
        get => _author;
        set => _author = value;
    }
    
    private string _isbn;
    [Key]
    [Column("ISBN")]
    [Required]
    [RegularExpression("^[0-9]{13}$", ErrorMessage = "ISBN Number must be 1-13 digits.")] //a number between 1-13 digits
    public string isbnNumber
    {
        get => _isbn;
        set => _isbn = value;
    }

    private string _publisher;
    [Required]
    [Column("Publisher")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Publisher name must be be 1-50 characters.")]
    public string Publisher
    {
        get => _publisher;
        set => _publisher = value;
    }

    private int _year;
    [Column("Year")]
    [Required]
    [Range(0, 2024, ErrorMessage = "Year must be between 0-2024.")]
    public int Year
    {
        get => _year;
        set => _year = value;
    }

    private int _buyPrice;
    [Column("BuyPrice")]
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Buy Price must be a positive number.")]
    public int BuyPrice
    {
        get => _buyPrice;
        set => _buyPrice = value;
    }

    private int _borrowPrice;
    [Column("BorrowPrice")]
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Borrow Price must be a positive number.")]
    public int BorrowPrice
    {
        get => _borrowPrice;
        set => _borrowPrice = value;
    }

    private int _ageLimit;
    [Column("AgeLimit")]
    [Range(0, 18, ErrorMessage = "Age Limit must be a number between 0-18.")]
    public int AgeLimit
    {
        get => _ageLimit;
        set => _ageLimit = value;
    }

    private string _format; //eBook formats - epub, f2b, mobi, pdf
    [Column("Format")]
    [Required]
    [RegularExpression("^(PDF|ePub|f2b|mobi)$")]
    public string Format
    {
        get => _format;
        set => _format = value;
    }

    private string _genre;
    [Column("Genre")]
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be be 1-50 characters.")]
    public string Genre
    {
        get => _genre;
        set => _genre = value;
    }
    
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