namespace eLibrary.Models;

public class CartItem
{
    public string ISBN { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Action { get; set; }
    public double Price { get; set; }
    public bool IsOnSale { get; set; }
    public int SalePercentage { get; set; }
    

    public CartItem()
    {
        
    }
    
    public CartItem(string isbn, string title, string author, string action, int price, bool isOnSale, int salePercentage)
    {
        ISBN = isbn;
        Title = title;
        Author = author;
        Action = action;
        if (isOnSale)
            Price = Math.Round(price * (100 - salePercentage) / 100.0, 2);
        else
            Price = price;
        IsOnSale = isOnSale;
        SalePercentage = salePercentage;
    }
}