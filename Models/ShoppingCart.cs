using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace eLibrary.Models
{
    public static class ShoppingCart
    {
        //private static ISession Session => new HttpContextAccessor().HttpContext?.Session; // Access the session in a static context
        private static readonly IHttpContextAccessor _context = new HttpContextAccessor();
        private static ISession Session => _context.HttpContext.Session;

        public static void Add(CartItem addItem)
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Add(addItem);

            var serializedCart = JsonSerializer.Serialize(shoppingCart);
            Session.SetString("ShoppingCart", serializedCart);
        }

        public static void Remove(CartItem removeItem)
        {
            var shoppingCart = GetShoppingCart();
            removeItem = shoppingCart.Find(item => item.ISBN == removeItem.ISBN);
            shoppingCart.Remove(removeItem);
            
            var serializedCart = JsonSerializer.Serialize(shoppingCart);
            Session.SetString("ShoppingCart", serializedCart);
        }

        public static List<CartItem> GetShoppingCart()
        {
            var serializedCart = Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(serializedCart))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(serializedCart) ?? new List<CartItem>();
        }

        public static double GetCartPrice()
        {
            var shoppingCart = GetShoppingCart();
            return shoppingCart.Sum(item => item.Price);
        }
        
        public static void ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            var serializedCart = JsonSerializer.Serialize(shoppingCart);
            Session.SetString("ShoppingCart", serializedCart);
        }
    }
}