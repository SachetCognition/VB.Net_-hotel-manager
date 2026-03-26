namespace HotelManagement.Core.DTOs;

public record CreateCheckInRequest(
    string GuestID, string RoomNo, decimal RoomCharges,
    DateTime DateIN, DateTime DateOUT, int NoOfAdults, int NoOfKids,
    string GuestName, string Address, string City, string ContactNo,
    string IDType, string IDNumber, decimal OtherCharges,
    decimal DiscountPer, decimal ServiceTaxPer, decimal LuxuryTaxPer,
    decimal TotalPaid, string ExtraBed, string Currency, string Notes);

public record CheckInResponse(
    int ID, string GuestID, string RoomNo, decimal RoomCharges,
    DateTime DateIN, DateTime DateOUT, int NoOfAdults, int NoOfKids,
    string GuestName, string Address, string City, string ContactNo,
    string IDType, string IDNumber, int NoOfDays,
    decimal TotalRoomCharges, decimal OtherCharges,
    decimal DiscountPer, decimal Discount, decimal SubTotal,
    decimal ServiceTaxPer, decimal ServiceTaxAmount,
    decimal LuxuryTaxPer, decimal LuxuryTaxAmount,
    decimal GrandTotal, decimal TotalPaid, decimal Balance,
    string ExtraBed, string Currency, string Status, string Notes);

public record CheckOutRequest(
    int CheckInId, decimal TotalPaid, string Currency);

public record CheckOutResponse(
    int ID, string BillNo, string GuestID, string RoomNo,
    decimal RoomCharges, DateTime DateIN, DateTime DateOUT,
    int NoOfDays, decimal TotalRoomCharges, decimal OtherCharges,
    decimal DiscountPer, decimal Discount, decimal SubTotal,
    decimal ServiceTaxPer, decimal ServiceTaxAmount,
    decimal LuxuryTaxPer, decimal LuxuryTaxAmount,
    decimal EducessTax, decimal EducessTaxAmount,
    decimal HEducessTax, decimal HEducessTaxAmount,
    decimal GrandTotal, decimal TotalPaid, decimal Balance,
    string ExtraBed, string Currency, string Status, DateTime CheckOutDate);

public record TaxCalculationResult(
    int NoOfDays, decimal TotalRoomCharges, decimal Discount,
    decimal SubTotal, decimal ServiceTaxAmount, decimal LuxuryTaxAmount,
    decimal GrandTotal, decimal Balance);

public record CheckOutTaxResult(
    decimal EducessTax, decimal EducessTaxAmount,
    decimal HEducessTax, decimal HEducessTaxAmount);
