using HotelManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly HotelDbContext _context;
    private readonly IConfiguration _configuration;

    public BackupController(HotelDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("backup")]
    public async Task<IActionResult> Backup([FromQuery] string? path = null)
    {
        var backupPath = path ?? Path.Combine(Path.GetTempPath(), $"HotelManagement_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var sql = $"BACKUP DATABASE [{databaseName}] TO DISK = @path WITH FORMAT, INIT";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@path", backupPath);
        command.CommandTimeout = 300;
        await command.ExecuteNonQueryAsync();

        return Ok(new { Message = "Backup completed successfully", Path = backupPath });
    }

    [HttpPost("restore")]
    public async Task<IActionResult> Restore([FromQuery] string path)
    {
        if (!System.IO.File.Exists(path))
            return BadRequest(new { Message = "Backup file not found" });

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        // Connect to master to restore
        builder.InitialCatalog = "master";
        using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        // Set single user mode
        var setSingle = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        using (var cmd = new SqlCommand(setSingle, connection))
        {
            cmd.CommandTimeout = 60;
            try { await cmd.ExecuteNonQueryAsync(); } catch { /* DB might not exist */ }
        }

        // Restore
        var sql = $"RESTORE DATABASE [{databaseName}] FROM DISK = @path WITH REPLACE";
        using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@path", path);
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync();
        }

        // Set multi user mode
        var setMulti = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";
        using (var cmd = new SqlCommand(setMulti, connection))
        {
            cmd.CommandTimeout = 60;
            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { Message = "Restore completed successfully" });
    }
}
