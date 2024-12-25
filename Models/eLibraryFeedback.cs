using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Models;

[Table("eLibraryFeedback")]
public class eLibraryFeedback
{
    private string _email;
    [Key]
    [EmailAddress]
    [Required]
    [Column(Order = 0)]
    public string Email
    {
        get => _email;
        set => _email = value;
    }

    private int _stars;
    [Key]
    [Required]
    [Column(Order = 1)]
    public int Stars 
    {
        get => _stars;
        set => _stars = value;
    }

    private string _content;
    [Key]
    [Column(Order = 2)]
    public string Content 
    {
        get => _content;
        set => _content = value;
    }

    // Foreign key to the Users table
    public virtual User User
    {
        get;
        set;
    }
    
    public eLibraryFeedback()
    {
       
    }

    public eLibraryFeedback(string email, int stars, string content)
    {
        Email = email;
        Stars = stars;
        Content = content;
    }
}