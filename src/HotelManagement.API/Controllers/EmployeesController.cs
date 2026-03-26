using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IExcelExportService _excelExportService;

    public EmployeesController(IEmployeeService employeeService, IExcelExportService excelExportService)
    {
        _employeeService = employeeService;
        _excelExportService = excelExportService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request)
    {
        var result = await _employeeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.EmployeeID }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(string id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetAll([FromQuery] string? search = null)
    {
        return Ok(await _employeeService.GetAllAsync(search));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponse>> Update(string id, UpdateEmployeeRequest request)
    {
        return Ok(await _employeeService.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _employeeService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("departments")]
    public async Task<ActionResult<IEnumerable<string>>> GetDepartments()
    {
        return Ok(await _employeeService.GetDepartmentsAsync());
    }

    [HttpGet("designations")]
    public async Task<ActionResult<IEnumerable<string>>> GetDesignations()
    {
        return Ok(await _employeeService.GetDesignationsAsync());
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search = null)
    {
        var data = await _employeeService.GetAllAsync(search);
        var bytes = _excelExportService.ExportToExcel(data.ToList(), "Employees");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
    }
}
