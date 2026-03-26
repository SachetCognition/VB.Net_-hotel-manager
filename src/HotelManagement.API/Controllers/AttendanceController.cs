using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceResponse>> Create(AttendanceRequest request)
    {
        var result = await _attendanceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttendanceResponse>> GetById(int id)
    {
        var result = await _attendanceService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceResponse>>> GetAll(
        [FromQuery] string? employeeId = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _attendanceService.GetAllAsync(employeeId, fromDate, toDate));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _attendanceService.DeleteAsync(id);
        return NoContent();
    }
}
