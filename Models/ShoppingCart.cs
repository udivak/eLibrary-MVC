using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace eLibrary.Models
{
    public static class ShoppingCart
    {
        private static ISession Session => new HttpContextAccessor().HttpContext?.Session; // Access the session in a static context

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

        public static int GetCartPrice()
        {
            var shoppingCart = GetShoppingCart();
            int totalPrice = 0;
            foreach (CartItem item in shoppingCart)
            {
                if (item == null)
                    continue;
                totalPrice += item.Quantity * item.Price;
            }
            return totalPrice;
        }
    }
}