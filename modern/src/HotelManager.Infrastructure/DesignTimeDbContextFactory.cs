using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HotelManager.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    /// <summary>
    /// Design-time factory used by <c>dotnet ef</c>. Select the provider with the
    /// <c>HotelManager_DbProvider</c> environment variable (<c>Sqlite</c> default or
    /// <c>Postgres</c>) so migrations can be generated for either provider, e.g.:
    /// <c>HotelManager_DbProvider=Postgres dotnet ef migrations add Name \
    ///   --output-dir Migrations/Postgres --namespace HotelManager.Infrastructure.Migrations.Postgres</c>.
    /// </summary>
    public HotelDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("HotelManager_DbProvider") ?? "Sqlite";
        var builder = new DbContextOptionsBuilder<HotelDbContext>();

        if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
        {
            NpgsqlCompat.EnableLegacyTimestampBehavior();
            var connectionString = Environment.GetEnvironmentVariable("HotelManager_PostgresConnectionString")
                ?? "Host=localhost;Port=5432;Database=hotelmanager;Username=postgres;Password=postgres";
            builder.UseNpgsql(connectionString);
        }
        else
        {
            builder.UseSqlite($"Data Source={DbPaths.DefaultDbPath}");
        }

        builder.ReplaceService<IMigrationsAssembly, ProviderFilteredMigrationsAssembly>();
        return new HotelDbContext(builder.Options);
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
