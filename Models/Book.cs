namespace eLibrary.Models;

public class Book
{
    private string title;
    private string author;
    private string isbnNumber;
    private int year;

    public Book(string title, string author, string ISBN, int year)
    {
        this.title = title;
        this.author = author;
        this.ISBN = ISBN;
        this.year = year;
    }
    
    public string Title { get => title; set => title = value; }
    public string Author { get => author; set => author = value; }
    public string ISBN { get => isbnNumber; set => isbnNumber = value; }
    public int Year { get => year; set => year = value; }
    
}