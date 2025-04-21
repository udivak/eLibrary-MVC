using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eLibrary.Models;

[Table("users2")]
public class User2
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
    
    private string _id;
    [Required]
    public string ID
    {
        get => _id;
        set => _id = value;
    }
    
    private string _role;
    [Required]
    public string Role
    {
        get => _role;
        set => _role = value;
    }
    
    private string _credit_card_number;
    [Required]
    public string CreditCardNumber
    {
        get => _credit_card_number;
        set => _credit_card_number = value;
    }
    
    private string _valid_date;
    [Required]
    public string ValidDate
    {
        get => _valid_date;
        set => _valid_date = value;
    }
    
    private int _cvc;
    [Required]
    public int CVC
    {
        get => _cvc;
        set => _cvc = value;
    }
    
    private string _email;
    [EmailAddress]
    [Required]
    [RegularExpression("^\\w+@\\w+\\.com$", ErrorMessage = "Email must contain @ and end with '.com'.")]
    public string Email
    {
        get => _email;
        set => _email = value;
    }

    private string _password;
    [Required]
    public string Password
    {
        get => _password;
        set => _password = value;
    }
    
    public User2()
    {
        //default ctor
    }

    public User2(string firstName, string lastName, string id, string role, string credit_card_number,
        string valid_date, int cvc, string email, string password)
    {
        FirstName = firstName;
        LastName = lastName;
        ID = id;
        Role = role;
        CreditCardNumber = credit_card_number;
        ValidDate = valid_date;
        CVC = cvc;
        Email = email;
        Password = password;
    }
}