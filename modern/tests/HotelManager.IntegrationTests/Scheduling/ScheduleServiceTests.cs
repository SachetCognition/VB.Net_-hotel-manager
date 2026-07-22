using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using HotelManager.Web.Services.Scheduling;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Scheduling;

/// <summary>
/// Per-test in-memory SQLite fixture owned by Child C so each scheduling test
/// gets an isolated database.
/// </summary>
public sealed class ScheduleDbFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<HotelDbContext> _options;

    public ScheduleDbFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var db = new HotelDbContext(_options);
        db.Database.EnsureCreated();
    }

    public HotelDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

public class ScheduleServiceTests : IDisposable
{
    private readonly ScheduleDbFixture _fixture = new();
    private readonly HotelDbContext _db;
    private readonly ScheduleService _service;

    public ScheduleServiceTests()
    {
        _db = _fixture.CreateContext();
        _service = new ScheduleService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _fixture.Dispose();
    }

    private static Appointment New(string subject, DateTime start, int hours = 1) => new()
    {
        Subject = subject,
        StartDate = start,
        EndDate = start.AddHours(hours),
        Location = "Conference Room",
        Status = 2,
    };

    [Fact]
    public async Task SaveAppointment_CreatesAndReads()
    {
        var saved = await _service.SaveAppointmentAsync(New("Staff meeting", new DateTime(2026, 1, 10, 9, 0, 0)));
        Assert.True(saved.ID > 0);

        var all = await _service.GetAppointmentsAsync();
        Assert.Single(all);
        Assert.Equal("Staff meeting", all[0].Subject);
    }

    [Fact]
    public async Task SaveAppointment_UpdatesExisting()
    {
        var saved = await _service.SaveAppointmentAsync(New("Old", new DateTime(2026, 1, 10, 9, 0, 0)));
        saved.Subject = "Updated";
        saved.Status = 0;
        await _service.SaveAppointmentAsync(saved);

        var fetched = await _service.GetAppointmentAsync(saved.ID);
        Assert.NotNull(fetched);
        Assert.Equal("Updated", fetched!.Subject);
        Assert.Equal(0, fetched.Status);
        Assert.Single(await _service.GetAppointmentsAsync());
    }

    [Fact]
    public async Task SaveAppointment_RequiresSubject()
    {
        var appt = New("", new DateTime(2026, 1, 10, 9, 0, 0));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SaveAppointmentAsync(appt));
    }

    [Fact]
    public async Task SaveAppointment_RejectsEndBeforeStart()
    {
        var appt = New("Bad range", new DateTime(2026, 1, 10, 9, 0, 0));
        appt.EndDate = appt.StartDate.AddHours(-2);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SaveAppointmentAsync(appt));
    }

    [Fact]
    public async Task GetAppointments_FiltersByDateRange()
    {
        await _service.SaveAppointmentAsync(New("January", new DateTime(2026, 1, 10, 9, 0, 0)));
        await _service.SaveAppointmentAsync(New("March", new DateTime(2026, 3, 10, 9, 0, 0)));

        var jan = await _service.GetAppointmentsAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31, 23, 59, 59));
        Assert.Single(jan);
        Assert.Equal("January", jan[0].Subject);
    }

    [Fact]
    public async Task GetAppointments_FiltersBySearch()
    {
        await _service.SaveAppointmentAsync(New("Board review", new DateTime(2026, 1, 10, 9, 0, 0)));
        await _service.SaveAppointmentAsync(New("Lunch", new DateTime(2026, 1, 11, 12, 0, 0)));

        var results = await _service.GetAppointmentsAsync(search: "Board");
        Assert.Single(results);
        Assert.Equal("Board review", results[0].Subject);
    }

    [Fact]
    public async Task DeleteAppointment_RemovesRecord()
    {
        var saved = await _service.SaveAppointmentAsync(New("Temp", new DateTime(2026, 1, 10, 9, 0, 0)));
        await _service.DeleteAppointmentAsync(saved.ID);
        Assert.Empty(await _service.GetAppointmentsAsync());
    }

    [Fact]
    public async Task DeleteAppointment_ThrowsWhenMissing()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAppointmentAsync(999));
    }
}
