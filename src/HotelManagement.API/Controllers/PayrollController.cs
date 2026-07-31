using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpPost("process")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment(ProcessPaymentRequest request)
    {
        var result = await _payrollService.ProcessPaymentAsync(request);
        return CreatedAtAction(nameof(GetPaymentById), new { id = result.ID }, result);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetPayments(
        [FromQuery] string? employeeId = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _payrollService.GetPaymentsAsync(employeeId, fromDate, toDate));
    }

    [HttpGet("payments/{id}")]
    public async Task<ActionResult<PaymentResponse>> GetPaymentById(int id)
    {
        var result = await _payrollService.GetPaymentByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("payments/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        await _payrollService.DeletePaymentAsync(id);
        return NoContent();
    }

    [HttpPost("advance")]
    public async Task<ActionResult<AdvanceEntryResponse>> CreateAdvance(AdvanceEntryRequest request)
    {
        return Ok(await _payrollService.CreateAdvanceEntryAsync(request));
    }

    [HttpGet("advance")]
    public async Task<ActionResult<IEnumerable<AdvanceEntryResponse>>> GetAdvanceEntries(
        [FromQuery] string? employeeId = null)
    {
        return Ok(await _payrollService.GetAdvanceEntriesAsync(employeeId));
    }

    [HttpGet("advance/balance/{employeeId}")]
    public async Task<ActionResult<decimal>> GetAdvanceBalance(string employeeId)
    {
        return Ok(await _payrollService.GetAdvanceBalanceAsync(employeeId));
    }
}
