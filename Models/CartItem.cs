namespace eLibrary.Models;

public class CartItem
{
    public string ISBN { get; set; }
    public string Title { get; set; }
    public string Action { get; set; }
    public double Price { get; set; }
    public int Quantity { get; set; }
    public bool IsOnSale { get; set; }
    public int SalePercentage { get; set; }

    public CartItem()
    {
        
    }
    
    public CartItem(string isbn, string title, string action, int price, int quantity, bool isOnSale, int salePercentage)
    {
        ISBN = isbn;
        Title = title;
        Action = action;
        if (isOnSale)
            Price = Math.Round(price * (100 - salePercentage) / 100.0, 2);
        else
            Price = price;
        Quantity = quantity;
        IsOnSale = isOnSale;
        SalePercentage = salePercentage;
    }
}