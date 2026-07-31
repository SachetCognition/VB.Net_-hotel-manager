using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckOutController : ControllerBase
{
    private readonly ICheckOutService _checkOutService;

    public CheckOutController(ICheckOutService checkOutService)
    {
        _checkOutService = checkOutService;
    }

    [HttpPost]
    public async Task<ActionResult<CheckOutResponse>> CheckOut(CheckOutRequest request)
    {
        var result = await _checkOutService.CheckOutAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CheckOutResponse>> GetById(int id)
    {
        var result = await _checkOutService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CheckOutResponse>>> GetAll(
        [FromQuery] string? roomNo = null, [FromQuery] string? guestName = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _checkOutService.GetAllAsync(roomNo, guestName, fromDate, toDate));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _checkOutService.DeleteAsync(id);
        return NoContent();
    }
}
