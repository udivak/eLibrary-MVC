namespace eLibrary.Models;

public class User
{
    private string firstName;
    private string lastName;
    private string username;
    private string password;
    private string email;
    private string created_at;
    private string address;
    private bool isAdmin = false;

    public User()
    {
        //default ctor
    }
    public User(string firstName, string lastName, string username, string password, string email, string createdAt,
                string address)
    {
        // if (validateUserName())
        //     this.username = username;
        // else
        //    Console.WriteLine("Error in username"); 
        //
        // if (validatePassword())
        //     this.password = password;
        // else
        // {
        //     Console.WriteLine("Error in password");
        // }
        this.firstName = firstName;
        this.lastName = lastName;
        this.username = username;
        this.password = password;
        this.email = email;
        this.created_at = createdAt;
        this.address = address;
    }
/*
    private bool validateUserName()
    {
        if (username.Length < 6 || username.Length > 8)
        {
            //MessageBox.Show("שם משתמש חייב להכיל בין 6-8 תווים", "שם משתמש לא תקין", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        int digitCount = username.Count(char.IsDigit);
        int letterCount = username.Count(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));

        if (digitCount > 3 || letterCount + digitCount != username.Length)
        {
           // MessageBox.Show("שם משתמש חייב להכיל עד 2 ספרות והשאר אותיות באנגלית", "שם משתמש לא תקין", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }
    private bool validatePassword()
    {
        if (password.Length < 8 || password.Length > 10)
        {
            //MessageBox.Show("הסיסמא חייבת להכיל בין 8-10 תווים", "סיסמא לא תקינה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit) || !password.Any(c => "!$#@%^&*".Contains(c)))
        {
            //MessageBox.Show("\u200F" + "הסיסמא חייבת להכיל לפחות אות אחת, ספרה אחת, ותו אחד מיוחד (!$#@%^&*)", "סיסמא לא תקינה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }
    */
    public string FirstName { get => firstName; set => firstName = value; }
    public string LastName { get => lastName; set => lastName = value; }
    public string UserName { get => username; set => username = value; }
    public string Password { get => password; set => password = value; }
    public string Email { get => email; set => email = value; }
    public string CreatedAt { get => created_at; set => created_at = value; }
    public string Address { get => address; set => address = value; }
    public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
}