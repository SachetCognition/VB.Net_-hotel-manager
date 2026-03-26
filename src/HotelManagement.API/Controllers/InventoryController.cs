using System.Security.Claims;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    // --- Dishes ---
    [HttpPost("dishes")]
    public async Task<ActionResult<DishResponse>> CreateDish(CreateDishRequest request)
    {
        return Ok(await _inventoryService.CreateDishAsync(request));
    }

    [HttpGet("dishes")]
    public async Task<ActionResult<IEnumerable<DishResponse>>> GetDishes([FromQuery] string? search = null)
    {
        return Ok(await _inventoryService.GetDishesAsync(search));
    }

    [HttpGet("dishes/{id}")]
    public async Task<ActionResult<DishResponse>> GetDish(int id)
    {
        var result = await _inventoryService.GetDishByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("dishes/{id}")]
    public async Task<ActionResult<DishResponse>> UpdateDish(int id, UpdateDishRequest request)
    {
        return Ok(await _inventoryService.UpdateDishAsync(id, request));
    }

    [HttpDelete("dishes/{id}")]
    public async Task<IActionResult> DeleteDish(int id)
    {
        await _inventoryService.DeleteDishAsync(id);
        return NoContent();
    }

    // --- Beer ---
    [HttpPost("beers")]
    public async Task<ActionResult<BeerResponse>> CreateBeer(CreateBeerRequest request)
    {
        return Ok(await _inventoryService.CreateBeerAsync(request));
    }

    [HttpGet("beers")]
    public async Task<ActionResult<IEnumerable<BeerResponse>>> GetBeers([FromQuery] string? search = null)
    {
        return Ok(await _inventoryService.GetBeersAsync(search));
    }

    [HttpPut("beers/{id}")]
    public async Task<ActionResult<BeerResponse>> UpdateBeer(int id, CreateBeerRequest request)
    {
        return Ok(await _inventoryService.UpdateBeerAsync(id, request));
    }

    [HttpDelete("beers/{id}")]
    public async Task<IActionResult> DeleteBeer(int id)
    {
        await _inventoryService.DeleteBeerAsync(id);
        return NoContent();
    }

    // --- Liquor ---
    [HttpPost("liquors")]
    public async Task<ActionResult<LiquorResponse>> CreateLiquor(CreateLiquorRequest request)
    {
        return Ok(await _inventoryService.CreateLiquorAsync(request));
    }

    [HttpGet("liquors")]
    public async Task<ActionResult<IEnumerable<LiquorResponse>>> GetLiquors([FromQuery] string? search = null)
    {
        return Ok(await _inventoryService.GetLiquorsAsync(search));
    }

    [HttpPut("liquors/{id}")]
    public async Task<ActionResult<LiquorResponse>> UpdateLiquor(int id, CreateLiquorRequest request)
    {
        return Ok(await _inventoryService.UpdateLiquorAsync(id, request));
    }

    [HttpDelete("liquors/{id}")]
    public async Task<IActionResult> DeleteLiquor(int id)
    {
        await _inventoryService.DeleteLiquorAsync(id);
        return NoContent();
    }

    // --- Liquor Master ---
    [HttpPost("liquor-masters")]
    public async Task<ActionResult<LiquorMasterResponse>> CreateLiquorMaster(CreateLiquorMasterRequest request)
    {
        return Ok(await _inventoryService.CreateLiquorMasterAsync(request));
    }

    [HttpGet("liquor-masters")]
    public async Task<ActionResult<IEnumerable<LiquorMasterResponse>>> GetLiquorMasters()
    {
        return Ok(await _inventoryService.GetLiquorMastersAsync());
    }

    [HttpDelete("liquor-masters/{id}")]
    public async Task<IActionResult> DeleteLiquorMaster(int id)
    {
        await _inventoryService.DeleteLiquorMasterAsync(id);
        return NoContent();
    }

    // --- Purchase Inventory ---
    [HttpPost("purchases")]
    public async Task<ActionResult<PurchaseResponse>> CreatePurchase(CreatePurchaseRequest request)
    {
        return Ok(await _inventoryService.CreatePurchaseAsync(request));
    }

    [HttpGet("purchases")]
    public async Task<ActionResult<IEnumerable<PurchaseResponse>>> GetPurchases(
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _inventoryService.GetPurchasesAsync(fromDate, toDate));
    }

    // --- Stock ---
    [HttpGet("stock")]
    public async Task<ActionResult<IEnumerable<StockResponse>>> GetStock()
    {
        return Ok(await _inventoryService.GetStockAsync());
    }

    [HttpPut("stock/{id}")]
    public async Task<IActionResult> UpdateStock(int id, UpdateStockRequest request)
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        await _inventoryService.UpdateStockAsync(id, request, userRole);
        return Ok();
    }
}
