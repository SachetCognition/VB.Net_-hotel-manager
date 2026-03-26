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
public class HallGardenController : ControllerBase
{
    private readonly HotelDbContext _context;

    public HallGardenController(HotelDbContext context)
    {
        _context = context;
    }

    // --- Halls ---
    [HttpGet("halls")]
    public async Task<ActionResult<IEnumerable<HallResponse>>> GetHalls()
    {
        var halls = await _context.Halls.OrderBy(h => h.HallName).ToListAsync();
        return Ok(halls.Select(h => new HallResponse(h.ID, h.HallName, h.Charges, h.Description)));
    }

    [HttpPost("halls")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HallResponse>> CreateHall(HallRequest request)
    {
        var hall = new Hall { HallName = request.HallName, Charges = request.Charges, Description = request.Description ?? string.Empty };
        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();
        return Ok(new HallResponse(hall.ID, hall.HallName, hall.Charges, hall.Description));
    }

    [HttpPut("halls/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HallResponse>> UpdateHall(int id, HallRequest request)
    {
        var hall = await _context.Halls.FindAsync(id);
        if (hall == null) return NotFound();
        hall.HallName = request.HallName;
        hall.Charges = request.Charges;
        hall.Description = request.Description ?? string.Empty;
        await _context.SaveChangesAsync();
        return Ok(new HallResponse(hall.ID, hall.HallName, hall.Charges, hall.Description));
    }

    [HttpDelete("halls/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHall(int id)
    {
        var hall = await _context.Halls.FindAsync(id);
        if (hall == null) return NotFound();
        _context.Halls.Remove(hall);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- Gardens ---
    [HttpGet("gardens")]
    public async Task<ActionResult<IEnumerable<GardenResponse>>> GetGardens()
    {
        var gardens = await _context.Gardens.OrderBy(g => g.GardenName).ToListAsync();
        return Ok(gardens.Select(g => new GardenResponse(g.ID, g.GardenName, g.Charges, g.Description)));
    }

    [HttpPost("gardens")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GardenResponse>> CreateGarden(GardenRequest request)
    {
        var garden = new Garden { GardenName = request.GardenName, Charges = request.Charges, Description = request.Description ?? string.Empty };
        _context.Gardens.Add(garden);
        await _context.SaveChangesAsync();
        return Ok(new GardenResponse(garden.ID, garden.GardenName, garden.Charges, garden.Description));
    }

    [HttpPut("gardens/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GardenResponse>> UpdateGarden(int id, GardenRequest request)
    {
        var garden = await _context.Gardens.FindAsync(id);
        if (garden == null) return NotFound();
        garden.GardenName = request.GardenName;
        garden.Charges = request.Charges;
        garden.Description = request.Description ?? string.Empty;
        await _context.SaveChangesAsync();
        return Ok(new GardenResponse(garden.ID, garden.GardenName, garden.Charges, garden.Description));
    }

    [HttpDelete("gardens/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGarden(int id)
    {
        var garden = await _context.Gardens.FindAsync(id);
        if (garden == null) return NotFound();
        _context.Gardens.Remove(garden);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- Hall and Garden Reservations ---
    [HttpPost("reservations/hall-and-garden")]
    public async Task<IActionResult> CreateHallAndGardenReservation(CreateHallGardenReservationRequest request)
    {
        var reservation = new ReservationHallAndGarden
        {
            GuestID = request.GuestID,
            GuestName = request.GuestName,
            HallName = request.HallName,
            GardenName = request.GardenName,
            DateIN = request.DateIN,
            DateOUT = request.DateOUT,
            NoOfDaysHall = request.NoOfDaysHall,
            NoOfDaysGarden = request.NoOfDaysGarden,
            RateHall = request.RateHall,
            RateGarden = request.RateGarden,
            TotalHall = request.RateHall * request.NoOfDaysHall,
            TotalGarden = request.RateGarden * request.NoOfDaysGarden,
            OtherCharges = request.OtherCharges,
            DiscountPer = request.DiscountPer,
            ServiceTaxPer = request.ServiceTaxPer,
            LuxuryTaxPer = request.LuxuryTaxPer,
            TotalPaid = request.TotalPaid,
            Currency = request.Currency ?? string.Empty,
            Status = "Reserved",
            Notes = request.Notes ?? string.Empty
        };

        // Calculate totals
        var totalCharges = reservation.TotalHall + reservation.TotalGarden;
        var discount = (totalCharges + request.OtherCharges) * request.DiscountPer / 100m;
        var subTotal = totalCharges + request.OtherCharges - discount;
        var serviceTax = Math.Round(subTotal * request.ServiceTaxPer / 100m, 2);
        var luxuryTax = Math.Round((subTotal + serviceTax) * request.LuxuryTaxPer / 100m, 2);
        var grandTotal = subTotal + serviceTax + luxuryTax;

        reservation.Discount = discount;
        reservation.SubTotal = subTotal;
        reservation.ServiceTaxAmount = serviceTax;
        reservation.LuxuryTaxAmount = luxuryTax;
        reservation.GrandTotal = grandTotal;
        reservation.Balance = grandTotal - request.TotalPaid;

        _context.ReservationHallAndGardens.Add(reservation);
        await _context.SaveChangesAsync();
        return Ok(reservation);
    }

    [HttpGet("reservations/hall-and-garden")]
    public async Task<IActionResult> GetHallAndGardenReservations()
    {
        return Ok(await _context.ReservationHallAndGardens.OrderByDescending(r => r.ID).ToListAsync());
    }

    [HttpPost("reservations/hall-or-garden")]
    public async Task<IActionResult> CreateHallOrGardenReservation(CreateHallOrGardenReservationRequest request)
    {
        var reservation = new ReservationHallOrGarden
        {
            GuestID = request.GuestID,
            GuestName = request.GuestName,
            VenueName = request.VenueName,
            VenueType = request.VenueType,
            DateIN = request.DateIN,
            DateOUT = request.DateOUT,
            NoOfDays = request.NoOfDays,
            Rate = request.Rate,
            TotalCharges = request.Rate * request.NoOfDays,
            OtherCharges = request.OtherCharges,
            DiscountPer = request.DiscountPer,
            ServiceTaxPer = request.ServiceTaxPer,
            LuxuryTaxPer = request.LuxuryTaxPer,
            TotalPaid = request.TotalPaid,
            Currency = request.Currency ?? string.Empty,
            Status = "Reserved",
            Notes = request.Notes ?? string.Empty
        };

        var totalCharges = reservation.TotalCharges;
        var discount = (totalCharges + request.OtherCharges) * request.DiscountPer / 100m;
        var subTotal = totalCharges + request.OtherCharges - discount;
        var serviceTax = Math.Round(subTotal * request.ServiceTaxPer / 100m, 2);
        var luxuryTax = Math.Round((subTotal + serviceTax) * request.LuxuryTaxPer / 100m, 2);
        var eduCess = Math.Round(request.ServiceTaxPer * 2m / 100m, 2);
        var eduCessAmt = Math.Round(serviceTax * eduCess / 100m, 2);
        var hEduCess = Math.Round(request.ServiceTaxPer * 1m / 100m, 2);
        var hEduCessAmt = Math.Round(serviceTax * hEduCess / 100m, 2);
        var grandTotal = subTotal + serviceTax + luxuryTax;

        reservation.Discount = discount;
        reservation.SubTotal = subTotal;
        reservation.ServiceTaxAmount = serviceTax;
        reservation.LuxuryTaxAmount = luxuryTax;
        reservation.EducessTax = eduCess;
        reservation.EducessTaxAmount = eduCessAmt;
        reservation.HEducessTax = hEduCess;
        reservation.HEducessTaxAmount = hEduCessAmt;
        reservation.GrandTotal = grandTotal;
        reservation.Balance = grandTotal - request.TotalPaid;

        _context.ReservationHallOrGardens.Add(reservation);
        await _context.SaveChangesAsync();
        return Ok(reservation);
    }

    [HttpGet("reservations/hall-or-garden")]
    public async Task<IActionResult> GetHallOrGardenReservations()
    {
        return Ok(await _context.ReservationHallOrGardens.OrderByDescending(r => r.ID).ToListAsync());
    }
}
