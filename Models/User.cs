using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Models;
[Table("Users")]
public class User
{
    private string _firstName;
    [Required]
    public string FirstName
    {
        get => _firstName;
        set => _firstName = value;
    }

    private string _lastName;
    [Required]
    public string LastName
    {
        get => _lastName;
        set => _lastName = value;
    }

    private string _userName;
    [Required]
    public string UserName
    {
        get => _userName;
        set => _userName = value;
    }

    private string _password;
    [Required]
    public string Password
    {
        get => _password;
        set => _password = value;
    }

    private string _email;
    [Key]
    [EmailAddress]
    [Required]
    [RegularExpression("^\\w+@\\w+\\.com$", ErrorMessage = "Email must contain @ and end with '.com'.")]
    public string Email
    {
        get => _email;
        set => _email = value;
    }

    private string _createdAt;
    [Required]
    [RegularExpression(@"^(0[1-9]|[12][0-9]|3[01])\/(0[1-9]|1[0-2])\/\d{4}$", ErrorMessage = "Invalid date format. Please use DD/MM/YYYY.")]
    public string CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value;
    }

    private string _address;
    [Required]
    public string Address
    {
        get => _address;
        set => _address = value;
    }

    private int _isAdmin;
    [Required]
    public int IsAdmin
    {
        get => _isAdmin;
        set => _isAdmin = value;
    }

    public User()
    {
        //default ctor
    }
    public User(string firstName, string lastName, string username, string password, string email, string createdAt,
                string address)
    {
        FirstName = firstName;
        LastName = lastName;
        UserName = username;
        Password = password;
        Email = email;
        CreatedAt = createdAt;
        Address = address;
    }
}