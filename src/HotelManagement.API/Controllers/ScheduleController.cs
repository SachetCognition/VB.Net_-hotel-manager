using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly HotelDbContext _context;

    public ScheduleController(HotelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleResponse>>> GetAll(
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var query = _context.Schedules.AsQueryable();
        if (fromDate.HasValue) query = query.Where(s => s.StartTime >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(s => s.EndTime <= toDate.Value);
        var items = await query.OrderByDescending(s => s.ID).ToListAsync();
        return Ok(items.Select(s => new ScheduleResponse(s.ID, s.Subject, s.Location, s.StartTime,
            s.EndTime, s.Description, s.Label, s.Status)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleResponse>> GetById(int id)
    {
        var s = await _context.Schedules.FindAsync(id);
        if (s == null) return NotFound();
        return Ok(new ScheduleResponse(s.ID, s.Subject, s.Location, s.StartTime,
            s.EndTime, s.Description, s.Label, s.Status));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleResponse>> Create(ScheduleRequest request)
    {
        var schedule = new Schedule
        {
            Subject = request.Subject,
            Location = request.Location ?? string.Empty,
            StartTime = request.StartDate,
            EndTime = request.EndDate,
            Description = request.Description ?? string.Empty,
            Label = request.Label ?? string.Empty,
            Status = request.Status ?? "Scheduled"
        };
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = schedule.ID },
            new ScheduleResponse(schedule.ID, schedule.Subject, schedule.Location, schedule.StartTime,
                schedule.EndTime, schedule.Description, schedule.Label, schedule.Status));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduleResponse>> Update(int id, ScheduleRequest request)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null) return NotFound();
        schedule.Subject = request.Subject;
        schedule.Location = request.Location ?? string.Empty;
        schedule.StartTime = request.StartDate;
        schedule.EndTime = request.EndDate;
        schedule.Description = request.Description ?? string.Empty;
        schedule.Label = request.Label ?? string.Empty;
        schedule.Status = request.Status ?? schedule.Status;
        await _context.SaveChangesAsync();
        return Ok(new ScheduleResponse(schedule.ID, schedule.Subject, schedule.Location, schedule.StartTime,
            schedule.EndTime, schedule.Description, schedule.Label, schedule.Status));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null) return NotFound();
        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
