namespace HotelManager.Domain.Entities;

/// <summary>
/// Calendar appointment ported from the legacy DevExpress scheduler
/// (FrmSchedule/CustomAppointmentForm). The legacy scheduler persisted the
/// standard DevExpress appointment fields (subject, start/end, location,
/// description, all-day flag, status and label). Recurring appointments are
/// preserved via <see cref="RecurrenceInfo"/>, matching how DevExpress
/// serialized recurrence data into a single string column.
/// </summary>
public class Appointment
{
    public int ID { get; set; }
    public string? Subject { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AllDay { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }

    /// <summary>DevExpress appointment status (0 = Free, 1 = Tentative, 2 = Busy, 3 = OutOfOffice).</summary>
    public int Status { get; set; }

    /// <summary>DevExpress appointment label/colour category (0 = None, 1..10 = colour labels).</summary>
    public int Label { get; set; }

    /// <summary>Optional resource the appointment is grouped under (e.g. a room or staff member).</summary>
    public string? ResourceID { get; set; }

    /// <summary>Serialized DevExpress recurrence rule; null for a one-off appointment.</summary>
    public string? RecurrenceInfo { get; set; }
}
