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
    public async Task<IActionResult> Backup([FromQuery] string? filename = null)
    {
        var allowedDir = _configuration.GetValue<string>("Backup:AllowedDirectory")
            ?? Path.Combine(Path.GetTempPath(), "HotelManagement_Backups");
        Directory.CreateDirectory(allowedDir);

        var safeFilename = filename ?? $"HotelManagement_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        if (safeFilename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || safeFilename.Contains(".."))
            return BadRequest(new { Message = "Invalid filename" });

        var backupPath = Path.GetFullPath(Path.Combine(allowedDir, safeFilename));
        if (!backupPath.StartsWith(Path.GetFullPath(allowedDir)))
            return BadRequest(new { Message = "Invalid backup path" });

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
    public async Task<IActionResult> Restore([FromQuery] string filename)
    {
        var allowedDir = _configuration.GetValue<string>("Backup:AllowedDirectory")
            ?? Path.Combine(Path.GetTempPath(), "HotelManagement_Backups");

        if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || filename.Contains(".."))
            return BadRequest(new { Message = "Invalid filename" });

        var path = Path.GetFullPath(Path.Combine(allowedDir, filename));
        if (!path.StartsWith(Path.GetFullPath(allowedDir)))
            return BadRequest(new { Message = "Invalid restore path" });

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

        try
        {
            // Restore
            var sql = $"RESTORE DATABASE [{databaseName}] FROM DISK = @path WITH REPLACE";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@path", path);
                cmd.CommandTimeout = 300;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            // Always restore multi-user mode, even if RESTORE fails
            var setMulti = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";
            using (var cmd = new SqlCommand(setMulti, connection))
            {
                cmd.CommandTimeout = 60;
                try { await cmd.ExecuteNonQueryAsync(); } catch { /* best effort */ }
            }
        }

        return Ok(new { Message = "Restore completed successfully" });
    }
}
