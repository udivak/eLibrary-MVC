namespace eLibrary.Models;

public class Book
{
    private string title;
    private string author;
    private string isbnNumber;
    private string publisher;
    private int year;
    private int buyPrice;
    private int borrowPrice;
    private int ageLimit;
    private string format;          //eBook formats - epub, f2b, mobi, pdf

    public Book()
    {
        //default ctor    
    }
    
    public Book(string title, string author, string isbnNumber, string publisher, int year, int buyPrice, int borrowPrice,
        int ageLimit, string format)
    {
        this.title = title;
        this.author = author;
        this.isbnNumber = isbnNumber;
        this.publisher = publisher;
        this.year = year;
        this.buyPrice = buyPrice;
        this.borrowPrice = borrowPrice;
        this.ageLimit = ageLimit;
        this.format = format;
    }
    
    public string Title { get => title; set => title = value; }
    public string Author { get => author; set => author = value; }
    public string ISBN { get => isbnNumber; set => isbnNumber = value; }
    public string Publisher { get => publisher; set => publisher = value; }
    public int Year { get => year; set => year = value; }
    public int BuyPrice { get => buyPrice; set => buyPrice = value; }
    public int BorrowPrice { get => borrowPrice; set => borrowPrice = value; }
    public int AgeLimit { get => ageLimit; set => ageLimit = value; }
    public string Format { get => format; set => format = value; }
}