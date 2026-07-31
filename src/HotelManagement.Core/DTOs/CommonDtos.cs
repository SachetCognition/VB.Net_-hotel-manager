namespace HotelManagement.Core.DTOs;

public record HotelInfoRequest(
    string HotelName, string Address, string City, string State,
    string ZipCode, string Phone, string Email, string Website,
    string TIN, string ServiceTaxNo);

public record HotelInfoResponse(
    int ID, string HotelName, string Address, string City, string State,
    string ZipCode, string Phone, string Email, string Website,
    string TIN, string ServiceTaxNo);

public record CurrencyRequest(string CurrencyName, string Symbol);
public record CurrencyResponse(int ID, string CurrencyName, string Symbol);

public record HallRequest(string HallName, decimal Charges, string Description);
public record HallResponse(int ID, string HallName, decimal Charges, string Description);

public record GardenRequest(string GardenName, decimal Charges, string Description);
public record GardenResponse(int ID, string GardenName, decimal Charges, string Description);

public record ExtraBedRequest(string BedType, decimal Charges);
public record ExtraBedResponse(int ID, string BedType, decimal Charges);

public record TaxInfoRequest(string TaxName, decimal TaxPercentage, string Description);
public record TaxInfoResponse(int ID, string TaxName, decimal TaxPercentage, string Description);

public record ScheduleRequest(
    string Subject, string? Location, DateTime StartDate, DateTime EndDate,
    string? Description, string? Label, string? Status);
public record ScheduleResponse(
    int ID, string Subject, string Location, DateTime StartDate, DateTime EndDate,
    string Description, string Label, string Status);

public record BackupRequest(string BackupPath);
public record RestoreRequest(string BackupFilePath);
public record BackupResponse(bool Success, string Message, string FilePath);

public record DashboardResponse(
    List<CheckInSummary> CurrentCheckIns,
    List<ReservationSummary> CurrentReservations);

public record CheckInSummary(
    string RoomNo, string GuestID, string GuestName,
    DateTime DateIN, DateTime DateOUT);

public record ReservationSummary(
    string RoomNo, string GuestID, string GuestName,
    DateTime DateIN, DateTime DateOUT);

public record ApiErrorResponse(string Message, IDictionary<string, string[]>? Errors = null);
