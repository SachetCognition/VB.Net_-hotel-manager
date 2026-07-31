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
public class HotelInfoController : ControllerBase
{
    private readonly HotelDbContext _context;

    public HotelInfoController(HotelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<HotelInfoResponse>> Get()
    {
        var info = await _context.HotelInfos.FirstOrDefaultAsync();
        if (info == null) return NotFound();
        return Ok(new HotelInfoResponse(info.ID, info.HotelName, info.Address, info.City,
            info.State, info.ZipCode, info.Phone, info.Email, info.Website, info.TIN, info.ServiceTaxNo));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HotelInfoResponse>> Update(HotelInfoRequest request)
    {
        var info = await _context.HotelInfos.FirstOrDefaultAsync();
        if (info == null)
        {
            info = new HotelInfo();
            _context.HotelInfos.Add(info);
        }

        info.HotelName = request.HotelName;
        info.Address = request.Address;
        info.City = request.City;
        info.State = request.State;
        info.ZipCode = request.ZipCode;
        info.Phone = request.Phone;
        info.Email = request.Email;
        info.Website = request.Website;
        info.TIN = request.TIN;
        info.ServiceTaxNo = request.ServiceTaxNo;

        await _context.SaveChangesAsync();
        return Ok(new HotelInfoResponse(info.ID, info.HotelName, info.Address, info.City,
            info.State, info.ZipCode, info.Phone, info.Email, info.Website, info.TIN, info.ServiceTaxNo));
    }

    [HttpGet("tax")]
    public async Task<ActionResult<IEnumerable<TaxInfoResponse>>> GetTaxInfo()
    {
        var taxes = await _context.TaxInformations.ToListAsync();
        return Ok(taxes.Select(t => new TaxInfoResponse(t.ID, t.TaxName, t.TaxPercentage, t.Description)));
    }

    [HttpPost("tax")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TaxInfoResponse>> CreateTaxInfo(TaxInfoRequest request)
    {
        var tax = new TaxInformation
        {
            TaxName = request.TaxName,
            TaxPercentage = request.TaxPercentage,
            Description = request.Description ?? string.Empty
        };
        _context.TaxInformations.Add(tax);
        await _context.SaveChangesAsync();
        return Ok(new TaxInfoResponse(tax.ID, tax.TaxName, tax.TaxPercentage, tax.Description));
    }

    [HttpGet("extra-bed")]
    public async Task<ActionResult<IEnumerable<ExtraBedResponse>>> GetExtraBeds()
    {
        var beds = await _context.ExtraBeds.ToListAsync();
        return Ok(beds.Select(b => new ExtraBedResponse(b.ID, b.BedType, b.Charges)));
    }

    [HttpPost("extra-bed")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ExtraBedResponse>> CreateExtraBed(ExtraBedRequest request)
    {
        var bed = new ExtraBed { BedType = request.BedType, Charges = request.Charges };
        _context.ExtraBeds.Add(bed);
        await _context.SaveChangesAsync();
        return Ok(new ExtraBedResponse(bed.ID, bed.BedType, bed.Charges));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
        var checkIns = await _context.CheckInRooms
            .Where(c => c.Status == "Checked In")
            .Select(c => new CheckInSummary(c.RoomNo, c.GuestID, c.GuestName, c.DateIN, c.DateOUT))
            .ToListAsync();

        var reservations = await _context.Reservations
            .Where(r => r.Status == "Reserved")
            .Select(r => new ReservationSummary(r.RoomNo, r.GuestID, r.GuestName, r.DateIN, r.DateOUT))
            .ToListAsync();

        return Ok(new DashboardResponse(checkIns, reservations));
    }
}
