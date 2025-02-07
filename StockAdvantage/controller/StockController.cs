using Microsoft.AspNetCore.Mvc;
using StockAdvantage.Hubs;

namespace StockAdvantage.controller;

[ApiController]
[Route("api/stocks")]
public class StockController(StockService service) : ControllerBase
{
    [HttpGet("balance")]
    public IActionResult GetBalance()
    {
        return Ok(service.GetBalance());
    }

    [HttpGet("price/{symbol}")]
    public async Task<IActionResult> GetStockPrice(string symbol)
    {
        var price = await service.GetStockPriceAsync(symbol);
        return Ok(price);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> BuyStock([FromBody] BuyStockRequest request)
    {
        decimal stockPrice = await service.GetStockPriceAsync(request.Symbol);
        var success = await service.PurchaseStock(stockPrice, request.Quantity);
        if (success)
        {
            return Ok(new { Message = "Stock bought successfully" , Balance = service.GetBalance()});
        }
        return BadRequest(new { Message = "Stock bought did not success" });
    }

    [HttpPost("sell")]
    public async Task<IActionResult> SellStock([FromBody] BuyStockRequest request)
    {
        decimal stockPrice = await service.GetStockPriceAsync(request.Symbol);
        var success = await service.SellStock(stockPrice, request.Quantity);
        if (success)
        {
            return Ok(service.GetBalance());
        }
        return BadRequest(new { Message = "Selling off the stock failed" });
    }

    public class BuyStockRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}