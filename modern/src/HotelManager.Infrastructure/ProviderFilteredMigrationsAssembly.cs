using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

// MigrationsAssembly is the documented extension point for customizing migration
// discovery; deriving from it is required to filter migrations by provider.
#pragma warning disable EF1001

namespace HotelManager.Infrastructure;

/// <summary>
/// Keeps SQLite and PostgreSQL migrations side by side in a single assembly by
/// only surfacing the migrations (and model snapshot) that live in the namespace
/// matching the active database provider. Registered via
/// <c>ReplaceService&lt;IMigrationsAssembly, ProviderFilteredMigrationsAssembly&gt;()</c>.
/// </summary>
public class ProviderFilteredMigrationsAssembly : MigrationsAssembly
{
    public const string SqliteNamespace = "HotelManager.Infrastructure.Migrations.Sqlite";
    public const string PostgresNamespace = "HotelManager.Infrastructure.Migrations.Postgres";

    private readonly string _providerNamespace;
    private IReadOnlyDictionary<string, TypeInfo>? _migrations;
    private ModelSnapshot? _modelSnapshot;
    private bool _snapshotResolved;

    public ProviderFilteredMigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger)
    {
        var providerName = currentContext.Context.Database.ProviderName ?? string.Empty;
        _providerNamespace = providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
            ? PostgresNamespace
            : SqliteNamespace;
    }

    public override IReadOnlyDictionary<string, TypeInfo> Migrations =>
        _migrations ??= base.Migrations
            .Where(kvp => kvp.Value.Namespace == _providerNamespace)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public override ModelSnapshot? ModelSnapshot
    {
        get
        {
            if (!_snapshotResolved)
            {
                _snapshotResolved = true;
                var snapshotType = Assembly.GetTypes().FirstOrDefault(t =>
                    !t.IsAbstract
                    && typeof(ModelSnapshot).IsAssignableFrom(t)
                    && t.Namespace == _providerNamespace);
                _modelSnapshot = snapshotType is null
                    ? null
                    : (ModelSnapshot)Activator.CreateInstance(snapshotType)!;
            }

            return _modelSnapshot;
        }
    }
}
