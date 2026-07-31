using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpPost]
    public async Task<ActionResult<CheckInResponse>> CheckIn(CreateCheckInRequest request)
    {
        var result = await _checkInService.CheckInAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CheckInResponse>> GetById(int id)
    {
        var result = await _checkInService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CheckInResponse>>> GetAll(
        [FromQuery] string? roomNo = null, [FromQuery] string? guestName = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _checkInService.GetAllAsync(roomNo, guestName, fromDate, toDate));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _checkInService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("room-available")]
    public async Task<ActionResult<bool>> CheckRoomAvailability(
        [FromQuery] string roomNo, [FromQuery] DateTime dateIn, [FromQuery] DateTime dateOut)
    {
        return Ok(await _checkInService.IsRoomAvailableAsync(roomNo, dateIn, dateOut));
    }

    [HttpPost("calculate-tax")]
    public ActionResult<TaxCalculationResult> CalculateTax(
        [FromQuery] decimal roomCharges, [FromQuery] int noOfDays,
        [FromQuery] decimal otherCharges, [FromQuery] decimal discountPer,
        [FromQuery] decimal serviceTaxPer, [FromQuery] decimal luxuryTaxPer,
        [FromQuery] decimal totalPaid)
    {
        var result = _checkInService.CalculateTaxes(roomCharges, noOfDays, otherCharges,
            discountPer, serviceTaxPer, luxuryTaxPer, totalPaid);
        return Ok(result);
    }
}
