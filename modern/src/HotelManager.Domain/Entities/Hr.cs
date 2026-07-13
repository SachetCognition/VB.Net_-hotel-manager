namespace HotelManager.Domain.Entities;

public class EmployeeRegistration
{
    public string EmployeeID { get; set; } = null!;
    public string? EmployeeName { get; set; }
    public string? Address { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Bloodgroup { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public int? Salary { get; set; }
    public string? BasicWorkingTime { get; set; }
    public ICollection<EmployeeAttendance> Attendances { get; set; } = new List<EmployeeAttendance>();
    public ICollection<EmployeePayment> Payments { get; set; } = new List<EmployeePayment>();
    public ICollection<AdvanceEntry> Advances { get; set; } = new List<AdvanceEntry>();
}

public class EmployeeAttendance
{
    public int AttendanceID { get; set; }
    public DateTime? WorkingDate { get; set; }
    public string? EmployeeID { get; set; }
    public string? BasicWorkingTime { get; set; }
    public string? Status { get; set; }
    public string? InTime { get; set; }
    public string? OutTime { get; set; }
    public string? Overtime { get; set; }
    public EmployeeRegistration? Employee { get; set; }
}

public class EmployeePayment
{
    public string PaymentID { get; set; } = null!;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? EmployeeID { get; set; }
    public int? PresentDays { get; set; }
    public int? Salary { get; set; }
    public int? Advance { get; set; }
    public int? Deduction { get; set; }
    public string? Overtime { get; set; }
    public int? OvertimeRate { get; set; }
    public int? OverTimeAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? ModeOfPayment { get; set; }
    public string? PaymentModeDetails { get; set; }
    public int? NetPay { get; set; }
    public EmployeeRegistration? Employee { get; set; }
}

/// <summary>
/// Advance/deduction ledger. A row with Amount > 0 is an advance entry;
/// a row with Deduction > 0 is a deduction entry (legacy frmAdvance / frmDeductionEntryRecord).
/// </summary>
public class AdvanceEntry
{
    public int ID { get; set; }
    public string? EmployeeID { get; set; }
    public int? Amount { get; set; }
    public int? Deduction { get; set; }
    public DateTime? WorkingDate { get; set; }
    public EmployeeRegistration? Employee { get; set; }
}
