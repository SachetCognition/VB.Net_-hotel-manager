using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll(
        [FromQuery] string? guestId = null, [FromQuery] string? roomNo = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _orderService.GetOrdersAsync(guestId, roomNo, fromDate, toDate));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }

    [HttpPost("restaurant")]
    public async Task<ActionResult<RestaurantOrderResponse>> CreateRestaurant(CreateRestaurantOrderRequest request)
    {
        return Ok(await _orderService.CreateRestaurantOrderAsync(request));
    }

    [HttpGet("restaurant")]
    public async Task<ActionResult<IEnumerable<RestaurantOrderResponse>>> GetRestaurantOrders(
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _orderService.GetRestaurantOrdersAsync(fromDate, toDate));
    }

    [HttpGet("restaurant/{id}")]
    public async Task<ActionResult<RestaurantOrderResponse>> GetRestaurantById(int id)
    {
        var result = await _orderService.GetRestaurantOrderByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("restaurant/{id}")]
    public async Task<IActionResult> DeleteRestaurant(int id)
    {
        await _orderService.DeleteRestaurantOrderAsync(id);
        return NoContent();
    }
}
