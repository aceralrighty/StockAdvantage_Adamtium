using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using StockAdvantage.Components;

namespace StockAdvantage.Hubs;

public class StockService
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey = Environment.GetEnvironmentVariable("API_KEY");
    // private readonly string _apiKey = "";

    private decimal _balance = 50000m;
    
    public decimal GetBalance() => _balance;

    public async Task<bool> PurchaseStock(decimal stockPrice, int quantity)
    {
        decimal totalCost = stockPrice * quantity;
        if (_balance >=totalCost)
        {
            _balance -= totalCost;
            await _hubContext.Clients.All.SendAsync("UpdateBalance", _balance);
            return true;
        }
        return false;
    }

    public StockService(IHubContext<StockHub> hubContext, HttpClient httpClient)
    {
        _hubContext = hubContext;
        _httpClient = httpClient;
    }

    public async Task SendStockPriceUpdate(string symbol)
    {
        var price = await GetStockPriceAsync(symbol);
        await _hubContext.Clients.All.SendAsync("ReceiveStockPriceUpdate", symbol, price);
    }

    public async Task<decimal> GetStockPriceAsync(string symbol)
    {
        try
        {
            var url = $"https://apidojo-yahoo-finance-v1.p.rapidapi.com/stock/v2/get-summary?symbol={symbol}&region=US";
    
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            Console.WriteLine(symbol);
            request.Headers.Add("X-RapidAPI-Key", _apiKey);
            request.Headers.Add("X-RapidAPI-Host", "apidojo-yahoo-finance-v1.p.rapidapi.com");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
        
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Full Response Body: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed: {response.StatusCode} - {responseContent}");
            }

            using var jsonDoc = JsonDocument.Parse(responseContent);
            Console.WriteLine($"Parsed JSON Root: {jsonDoc.RootElement}");

            // Add more specific JSON path debugging
            var root = jsonDoc.RootElement;
            Console.WriteLine("JSON Structure:");
            foreach (var property in root.EnumerateObject())
            {
                Console.WriteLine($"Property: {property.Name}");
            }
            if (root.TryGetProperty("price", out var priceElement) &&
                priceElement.TryGetProperty("regularMarketPrice", out var marketPriceElement) &&
                marketPriceElement.TryGetProperty("raw", out var rawPriceElement))
            {
                string rawPriceString = rawPriceElement.ToString();
                if (decimal.TryParse(rawPriceString, out decimal stockPrice))
                {
                    return stockPrice;
                }
            }

            throw new Exception("Stock price not available.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw new Exception("An error occurred while fetching the stock price.", ex);
        }
    }
}