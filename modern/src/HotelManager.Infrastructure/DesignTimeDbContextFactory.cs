using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelManager.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite($"Data Source={DbPaths.DefaultDbPath}")
            .Options;
        return new HotelDbContext(options);
    }
}

public static class DbPaths
{
    /// <summary>Well-known local SQLite path: %LOCALAPPDATA%/HotelManager/hotelmanager.db (or ~/.hotelmanager on Unix).</summary>
    public static string DefaultDbPath
    {
        get
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(baseDir))
                baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hotelmanager");
            var dir = Path.Combine(baseDir, "HotelManager");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "hotelmanager.db");
        }
    }
}
