namespace eLibrary.Models;

public class CartItem
{
    public string ISBN { get; set; }
    public string Action { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }

    public CartItem()
    {
        
    }

    public CartItem(string isbn, string action, int price, int quantity)
    {
        ISBN = isbn;
        Action = action;
        Price = price;
        Quantity = quantity;
    }
}