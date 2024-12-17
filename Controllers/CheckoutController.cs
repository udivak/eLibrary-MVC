using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Controllers;

public class CheckoutController : Controller
{
    private string PaypalClientID { get; set; } = "";
    private string PaypalSecret { get; set; } = "";
    private string PaypalUrl { get; set; } = "";
    private DB_context _dbContext;
    private readonly IHttpContextAccessor _context;
    public CheckoutController(IConfiguration configuration, DB_context dbContext, IHttpContextAccessor context)
    {
        PaypalClientID = configuration["PaypalSettings:ClientID"]!;
        PaypalSecret = configuration["PaypalSettings:Secret"]!;
        PaypalUrl = configuration["PaypalSettings:Url"]!;
        _dbContext = dbContext;
        _context = context;
    }
    private ISession Session => _context.HttpContext.Session;
    
    public IActionResult CheckoutPage()
    {
        string serializedCart = Session.GetString("ShoppingCart");
        List<CartItem> cartItems = string.IsNullOrEmpty(serializedCart) ? new List<CartItem>() 
            : JsonSerializer.Deserialize<List<CartItem>>(serializedCart) ?? new List<CartItem>();
        ViewBag.PaypalClientID = PaypalClientID;
        return View("Checkout", cartItems);
    }
    public async Task<string> Token()                 //Test Func to see the PayPal Access Token
    {
        return await GetPayPalAccessToken();
    }                                
    public IActionResult Checkout()
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
    }

    [HttpPost]
    public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
    {
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
                        //save the order to db
                        return new JsonResult("success");
                    }
                }
            }
        }
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