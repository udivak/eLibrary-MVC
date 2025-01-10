using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaypalServerSdk.Standard.Models;

namespace eLibrary.Models;

[Table("BookReviews")]
public class BookReview
{
    private string _email;
    [Key]
    [EmailAddress]
    [Required]
    [Column("Email")]
    public string Email
    {
        get => _email;
        set => _email = value;
    }
    
    private string _isbn;
    [Key]
    [Column("ISBN")]
    [Required]
    [RegularExpression("^[0-9]{13}$", ErrorMessage = "ISBN Number must be 13 digits.")]
    public string ISBN
    {
        get => _isbn;
        set => _isbn = value;
    }
    
    private string _createdAt;
    [Column("CreatedAt")]
    public string CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value;
    }

    private string _title;
    [Column("Title")]
    public string Title
    {
        get => _title;
        set => _title = value;
    }
    
    private int _stars;
    [Required]
    [Column("Stars")]
    public int Stars 
    {
        get => _stars;
        set => _stars = value;
    }
    
    private string _content;
    [Required]
    [Column("Content")]
    public string Content 
    {
        get => _content;
        set => _content = value;
    }
    
    private string _userName;
    [Required]
    [Column("UserName")]
    public string UserName 
    {
        get => _userName;
        set => _userName = value;
    }
    
    public BookReview()
    {
       //default ctor
    }

    public BookReview(string email, string isbn, string title, int stars, string content, string userName)
    {
        Email = email;
        ISBN = isbn;
        CreatedAt = DateTime.Today.ToString("d");
        Stars = stars;
        Title = title;
        Content = content;
        UserName = userName;
    }
}