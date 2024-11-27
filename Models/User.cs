namespace eLibrary.Models;

public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string CreatedAt { get; set; }
    public string Address { get; set; }
    public bool IsAdmin { get; set; }

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
        FirstName = firstName;
        LastName = lastName;
        UserName = username;
        Password = password;
        Email = email;
        CreatedAt = createdAt;
        Address = address;
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

}