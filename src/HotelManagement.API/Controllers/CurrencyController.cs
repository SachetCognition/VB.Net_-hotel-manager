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
public class CurrencyController : ControllerBase
{
    private readonly HotelDbContext _context;

    public CurrencyController(HotelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrencyResponse>>> GetAll()
    {
        var currencies = await _context.CurrencySets.OrderBy(c => c.CurrencyName).ToListAsync();
        return Ok(currencies.Select(c => new CurrencyResponse(c.ID, c.CurrencyName, c.Symbol)));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CurrencyResponse>> Create(CurrencyRequest request)
    {
        var currency = new CurrencySet { CurrencyName = request.CurrencyName, Symbol = request.Symbol };
        _context.CurrencySets.Add(currency);
        await _context.SaveChangesAsync();
        return Ok(new CurrencyResponse(currency.ID, currency.CurrencyName, currency.Symbol));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var currency = await _context.CurrencySets.FindAsync(id);
        if (currency == null) return NotFound();
        _context.CurrencySets.Remove(currency);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
