namespace HotelManager.Infrastructure;

/// <summary>
/// PostgreSQL compatibility shims. The legacy schema stores wall-clock
/// <see cref="System.DateTime"/> values with <c>Kind=Local/Unspecified</c>; enabling
/// Npgsql's legacy timestamp behavior maps <c>DateTime</c> to <c>timestamp without time zone</c>
/// so those values round-trip without forcing UTC conversion. Must be called before the
/// Npgsql type mapping source is first built (i.e. before creating any Npgsql-backed context).
/// This only affects the PostgreSQL provider; SQLite is unaffected.
/// </summary>
public static class NpgsqlCompat
{
    public static void EnableLegacyTimestampBehavior()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}
