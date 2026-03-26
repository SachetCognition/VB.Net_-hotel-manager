using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request)
    {
        var result = await _reservationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id)
    {
        var result = await _reservationService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetAll(
        [FromQuery] string? roomNo = null, [FromQuery] string? guestName = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _reservationService.GetAllAsync(roomNo, guestName, fromDate, toDate));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReservationResponse>> Update(int id, CreateReservationRequest request)
    {
        return Ok(await _reservationService.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _reservationService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("check-overlap")]
    public async Task<ActionResult<bool>> CheckOverlap(
        [FromQuery] string roomNo, [FromQuery] DateTime dateIn, [FromQuery] DateTime dateOut)
    {
        return Ok(await _reservationService.CheckDateOverlapAsync(roomNo, dateIn, dateOut));
    }
}
