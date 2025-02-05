using Microsoft.AspNetCore.Mvc;
using StockAdvantage.Hubs;

namespace StockAdvantage.controller;

[ApiController]
[Route("api/stocks")]
public class StockController: ControllerBase
{
    private readonly StockService _service;

    public StockController(StockService service)
    {
        _service = service;
    }

    [HttpGet("balance")]
    public IActionResult GetBalance()
    {
        return Ok(_service.GetBalance());
    }

    [HttpGet("price/{symbol}")]
    public async Task<IActionResult> GetStockPrice(string symbol)
    {
        var price = await _service.GetStockPriceAsync(symbol);
        return Ok(price);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> BuyStock([FromBody] BuyStockRequest request)
    {
        decimal stockPrice = await _service.GetStockPriceAsync(request.Symbol);
        var success = await _service.PurchaseStock(stockPrice, request.Quantity);
        if (success)
        {
            return Ok(new { Message = "Stock bought successfully" , Balance = _service.GetBalance()});
        }
        return BadRequest(new { Message = "Stock bought did not success" });
    }

    public class BuyStockRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}