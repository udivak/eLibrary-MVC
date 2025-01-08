using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using eLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Controllers;

public class CheckoutController : Controller
{
    private string PaypalClientID { get; set; } = "";
    private string PaypalSecret { get; set; } = "";
    private string PaypalUrl { get; set; } = "";
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;
    private readonly IEmailService _emailService;

    public CheckoutController(IConfiguration configuration, DB_context dbContext, IHttpContextAccessor context, IEmailService emailService)
    {
        PaypalClientID = configuration["PaypalSettings:ClientID"]!;
        PaypalSecret = configuration["PaypalSettings:Secret"]!;
        PaypalUrl = configuration["PaypalSettings:Url"]!;
        _dbContext = dbContext;
        _context = context;
        _emailService = emailService;
    }
    private ISession Session => _context.HttpContext.Session;
    
    public IActionResult CheckoutPage()
    {
        string serializedCart = Session.GetString("ShoppingCart");
        List<CartItem> cartItems;
       if (string.IsNullOrEmpty(serializedCart))
       {
           cartItems = new List<CartItem>();
       }
       else
       {
           cartItems = JsonSerializer.Deserialize<List<CartItem>>(serializedCart);
           if (cartItems == null)                                        //Deserialize failed
           {
               cartItems = new List<CartItem>();
           }
       }
        ViewBag.PaypalClientID = PaypalClientID;
        return View("Checkout", cartItems);
    }

    public void RemoveFromCart(string isbn, string qty)
    {
        var book = _dbContext.Books.FirstOrDefault(b => b.ISBN == isbn);
        int quantity;
        try
        {
            quantity = int.Parse(qty);
        }
        catch (Exception)
        {
            quantity = 0;
        }
        //book.Quantity += quantity;
        _dbContext.SaveChangesAsync();
        List <CartItem> shoppingCart = ShoppingCart.GetShoppingCart();
        
    }
    
    /*public IActionResult Checkout()
    {
        // Fetch data to populate the cart
        List<Book> books = _dbContext.GetAllBooks().Take(4).ToList();
        if (books == null || !books.Any()) // Check if books are not null and not empty
        {
            return View("Checkout", new List<CartItem>()); // Return an empty cart view if no books found
        }
        List<CartItem> cart = new List<CartItem>();
        Random random = new Random();
        foreach (var book in books)
        {
            CartItem temp = new CartItem(book.isbnNumber, book.Title, "Buy", book.BuyPrice, random.Next(1, 6));
            cart.Add(temp);
        }
        // Ensure the session is populated before passing data to the view
        string serializedCart = JsonSerializer.Serialize(cart);
        Session.SetString("ShoppingCart", serializedCart);
        // Validate the cart items
        if (cart == null || !cart.Any())
        {
            return View("Checkout", new List<CartItem>());
        }
        return RedirectToAction("CheckoutPage", "Checkout");
    }*/
    
    public async Task<string> Token()                 //Test Func to see the PayPal Access Token
    {
        return await GetPayPalAccessToken();
    } 
    
    [HttpPost]
    public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
    {
        // string serializedCart = Session.GetString("ShoppingCart");
        // Console.WriteLine(serializedCart);
        // List<CartItem> cartItems = JsonSerializer.Deserialize<List<CartItem>>(serializedCart);
        // Console.WriteLine(cartItems);
        // Console.WriteLine(data);
        // Console.WriteLine(data.ToString());
        var totalAmount = data?["amount"]?.ToString();
        if (totalAmount == null)
        {
            return new JsonResult(new { Id = "" });
        }
        // create the request body
        JsonObject createOrderRequest = new JsonObject();
        createOrderRequest.Add("intent", "CAPTURE");
        JsonObject amount = new JsonObject();
        amount.Add("currency_code", "USD");
        amount.Add("value", totalAmount);

        JsonObject purchaseUnit1 = new JsonObject();
        purchaseUnit1.Add("amount", amount);

        JsonArray purchasedUnits = new JsonArray();
        purchasedUnits.Add(purchaseUnit1);
        
        createOrderRequest.Add("purchase_units", purchasedUnits);
        
        //get access token
        string accessToken = await GetPayPalAccessToken();
        //send request
        string url = PaypalUrl + "/v2/checkout/orders";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");
            var httpResponse = await client.SendAsync(requestMessage);

            if (httpResponse.IsSuccessStatusCode)
            {
                var strResponse = await httpResponse.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(strResponse);

                if (jsonResponse != null)
                {
                    string paypalOrderID = jsonResponse["id"]?.ToString() ?? "";
                    return new JsonResult(new { Id = paypalOrderID });
                }
            }
        }
        return new JsonResult(new { Id = "" });
    }
    
    [HttpPost]
    public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
    {
        var orderId = data?["orderID"]?.ToString();
        if (orderId == null)
        {
            return new JsonResult("error");
        }

        string accessToken = await GetPayPalAccessToken();
        string url = PaypalUrl + $"/v2/checkout/orders/{orderId}/capture";
        var userEmail = Session.GetString("userEmail");

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = new StringContent("", null, "application/json");
            var httpResponse = await client.SendAsync(requestMessage);
            if (httpResponse.IsSuccessStatusCode)
            {
                var strResponse = await httpResponse.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(strResponse);
                if (jsonResponse != null)
                {
                    string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                    if (paypalOrderStatus == "COMPLETED")
                    {
                        var shoppingCart = ShoppingCart.GetShoppingCart();
                        string emailBody = "<h1>Order Confirmation</h1><p>Thank you for your order!</p><ul>";
                        var totalPrice = 0;
                        foreach (CartItem item in shoppingCart)
                        {
                            var book = _dbContext.Books.FirstOrDefault(b => b.ISBN == item.ISBN);
                            bool isPurchased = item.Action == "Buy";
                            UserBook newUserBook = new UserBook(userEmail, item.ISBN, isPurchased);
                            _dbContext.UserBook.Add(newUserBook);
                            await _dbContext.SaveChangesAsync();
                            emailBody += $"<li>{book.Title} - 1 x {book.BuyPrice}$ = {book.BuyPrice}$</li>";
                            totalPrice += book.BuyPrice;
                        }
                        // double totalPrice = ShoppingCart.GetCartPrice();
                        emailBody += $"</ul><p>Total: {totalPrice}$</p>";
                        await _emailService.SendEmailAsync(
                            userEmail,
                            "Your Order Confirmation",
                            emailBody
                        );
                        ShoppingCart.ClearCart();
                        return new JsonResult("success");
                    }
                }
            }
        }
        ShoppingCart.ClearCart();
        return new JsonResult("success");
    }
    
    public async Task<string> GetPayPalAccessToken()
    {
        string accessToken = "";
        string url = PaypalUrl + "/v1/oauth2/token";
        using (var client = new HttpClient())
        {
            string credentials64 =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientID + ":" + PaypalSecret));
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64 );
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content =
                new StringContent("grant_type=client_credentials", null, "application/x-www-form-urlencoded");
            var httpResponse = await client.SendAsync(requestMessage);

            if (httpResponse.IsSuccessStatusCode)
            {
                var strResponse = await httpResponse.Content.ReadAsStringAsync();

                var jsonResponse = JsonNode.Parse(strResponse);
                if (jsonResponse != null)
                {
                    accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                }
            }
        }
        return accessToken;
    }
}