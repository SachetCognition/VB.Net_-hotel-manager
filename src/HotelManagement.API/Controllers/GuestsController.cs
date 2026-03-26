using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly IExcelExportService _excelExportService;

    public GuestsController(IGuestService guestService, IExcelExportService excelExportService)
    {
        _guestService = guestService;
        _excelExportService = excelExportService;
    }

    [HttpPost]
    public async Task<ActionResult<GuestResponse>> Create(CreateGuestRequest request)
    {
        var result = await _guestService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.GuestID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GuestResponse>> GetById(string id)
    {
        var result = await _guestService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestResponse>>> GetAll([FromQuery] string? search = null)
    {
        var result = await _guestService.GetAllAsync(search);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GuestResponse>> Update(string id, UpdateGuestRequest request)
    {
        var result = await _guestService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _guestService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search = null)
    {
        var data = await _guestService.GetAllAsync(search);
        var bytes = _excelExportService.ExportToExcel(data.ToList(), "Guests");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Guests.xlsx");
    }
}
