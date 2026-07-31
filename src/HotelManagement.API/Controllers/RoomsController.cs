using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomResponse>> Create(CreateRoomRequest request)
    {
        var result = await _roomService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.RoomNo }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponse>> GetById(string id)
    {
        var result = await _roomService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomResponse>>> GetAll([FromQuery] string? search = null)
    {
        return Ok(await _roomService.GetAllAsync(search));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomResponse>> Update(string id, UpdateRoomRequest request)
    {
        return Ok(await _roomService.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _roomService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<RoomResponse>>> GetAvailable(
        [FromQuery] DateTime dateIn, [FromQuery] DateTime dateOut)
    {
        return Ok(await _roomService.GetAvailableRoomsAsync(dateIn, dateOut));
    }
}
