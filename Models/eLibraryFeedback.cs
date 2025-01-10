using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace eLibrary.Models;

[Table("eLibraryFeedbacks")]
public class eLibraryFeedback
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
    
    private string _createdAt;
    [Key]
    [Required]
    [Column("CreatedAt")]
    public string CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value;
    }
    
    private int _stars;
    [Key]
    [Required]
    [Column("Stars")]
    public int Stars 
    {
        get => _stars;
        set => _stars = value;
    }

    private string _content;
    [Key]
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
    
    public eLibraryFeedback()
    {
       //default ctor
    }

    public eLibraryFeedback(string email, int stars, string content, string userName)
    {
        Email = email;
        CreatedAt = DateTime.Today.ToString("d");
        Stars = stars;
        Content = content;
        UserName = userName;
    }
}