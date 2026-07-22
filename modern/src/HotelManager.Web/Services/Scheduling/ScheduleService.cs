using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Scheduling;

/// <summary>
/// Calendar/scheduling service reconstructed from the legacy DevExpress
/// scheduler (FrmSchedule/CustomAppointmentForm). Persists appointments to the
/// database instead of the legacy in-memory SchedulerStorage, exposing the same
/// create/edit/delete operations the WinForms ribbon offered.
/// </summary>
public class ScheduleService : IScheduleService
{
    private readonly HotelDbContext _db;

    public ScheduleService(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsAsync(DateTime? from = null, DateTime? to = null, string? search = null)
    {
        var query = _db.Appointments.AsNoTracking();
        if (from is not null)
            query = query.Where(a => a.EndDate >= from);
        if (to is not null)
            query = query.Where(a => a.StartDate <= to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = (search ?? string.Empty) + "%";
            query = query.Where(a =>
                EF.Functions.Like(a.Subject!, pattern) ||
                EF.Functions.Like(a.Location!, pattern));
        }
        return await query.OrderBy(a => a.StartDate).ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentAsync(int id) =>
        await _db.Appointments.AsNoTracking().FirstOrDefaultAsync(a => a.ID == id);

    public async Task<Appointment> SaveAppointmentAsync(Appointment appointment)
    {
        if (string.IsNullOrWhiteSpace(appointment.Subject))
            throw new InvalidOperationException("Please enter a subject");
        if (appointment.EndDate < appointment.StartDate)
            throw new InvalidOperationException("End time cannot be earlier than start time");

        if (appointment.ID == 0)
        {
            _db.Appointments.Add(appointment);
        }
        else
        {
            var existing = await _db.Appointments.FindAsync(appointment.ID)
                ?? throw new InvalidOperationException("No record found");
            existing.Subject = appointment.Subject;
            existing.StartDate = appointment.StartDate;
            existing.EndDate = appointment.EndDate;
            existing.AllDay = appointment.AllDay;
            existing.Location = appointment.Location;
            existing.Description = appointment.Description;
            existing.Status = appointment.Status;
            existing.Label = appointment.Label;
            existing.ResourceID = appointment.ResourceID;
            existing.RecurrenceInfo = appointment.RecurrenceInfo;
            appointment = existing;
        }
        await _db.SaveChangesAsync();
        return appointment;
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id)
            ?? throw new InvalidOperationException("No record found");
        _db.Appointments.Remove(appointment);
        await _db.SaveChangesAsync();
    }
}
