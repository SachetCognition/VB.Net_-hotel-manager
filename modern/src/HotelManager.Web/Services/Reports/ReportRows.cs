namespace HotelManager.Web.Services.Reports;

// Row DTOs projected from the database for each legacy Crystal Report.
// Column sets mirror the SELECT statements the legacy forms fed into the
// corresponding .rpt files (all missing from the repo).

public record HotelHeaderRow(
    string? HotelName,
    string? Address,
    string? ContactNo,
    string? ContactNo1,
    string? Email,
    string? TIN,
    string? STNo);

/// <summary>rptInvoice_Room — CheckIN_Room + Checkout_Room + Guest + HotelInfo + CurrencySet.</summary>
public record RoomInvoiceRow(
    int CheckoutId,
    string? BillNo,
    DateTime? CheckOutDate,
    string? Currency,
    string? GuestID,
    string? GuestName,
    string? GuestAddress,
    string? City,
    string? GuestContactNo,
    string? IDType,
    string? IDNumber,
    string? RoomNo,
    int? RoomCharges,
    DateTime? DateIN,
    DateTime? DateOUT,
    int? NoOfAdults,
    int? NoOfKids,
    int? NoOfDays,
    string? ExtraBed,
    double? TotalRoomCharges,
    double? OtherCharges,
    double? SubTotal,
    double? ServiceTaxPer,
    double? ServiceTaxAmount,
    double? LuxuryTaxPer,
    double? LuxuryTaxAmount,
    double? DiscountPer,
    double? Discount,
    double? GrandTotal,
    double? TotalPaid,
    double? Balance,
    string? Status,
    string? Notes,
    HotelHeaderRow Hotel);

/// <summary>rptInvoice_HallandGarden — Reservation_Hall_and_Garden + Guest + HotelInfo + CurrencySet.</summary>
public record HallAndGardenInvoiceRow(
    string ReservationId,
    string? Currency,
    string? GuestID,
    string? GuestName,
    string? GuestAddress,
    string? City,
    string? GuestContactNo,
    string? Hall,
    DateTime? DateFromHall,
    DateTime? DateToHall,
    double? DaysHall,
    double? RateHall,
    double? TotalChargesHall,
    string? Garden,
    DateTime? DateFromGarden,
    DateTime? DateToGarden,
    int? DaysGarden,
    double? RateGarden,
    double? TotalChargesGarden,
    double? OtherCharges,
    double? SubTotal,
    double? ServiceTaxPer,
    double? ServiceTaxAmount,
    double? LuxuryTaxPer,
    double? LuxuryTaxAmount,
    double? DiscountPer,
    double? Discount,
    double? GrandTotal,
    double? TotalPaid,
    double? Balance,
    string? Notes,
    HotelHeaderRow Hotel);

/// <summary>rptInvoice_HallorGarden — Reservation_Hall_or_Garden + Guest + HotelInfo + CurrencySet.</summary>
public record HallOrGardenInvoiceRow(
    string ReservationId,
    string? Currency,
    string? GuestID,
    string? GuestName,
    string? GuestAddress,
    string? City,
    string? GuestContactNo,
    string? Type,
    DateTime? DateFrom,
    DateTime? DateTo,
    int? Days,
    double? Rate,
    double? TotalCharges,
    double? OtherCharges,
    double? SubTotal,
    double? ServiceTaxPer,
    double? ServiceTaxAmount,
    double? LuxuryTaxPer,
    double? LuxuryTaxAmount,
    double? DiscountPer,
    double? Discount,
    double? GrandTotal,
    double? TotalPaid,
    double? Balance,
    string? Notes,
    HotelHeaderRow Hotel);

public record OrderLineRow(
    string? ProductID,
    string? ProductName,
    int? Volume,
    int? Rate,
    int? Quantity,
    int? Amount);

/// <summary>rptOrderInvoice — OrderInfo + Ordered_Product + CheckIN_Room + Guest + HotelInfo + CurrencySet.</summary>
public record OrderInvoiceRow(
    int OrderId,
    string? OrderNo,
    DateTime? OrderDate,
    string? Currency,
    string? RoomNo,
    string? GuestID,
    string? GuestName,
    string? GuestAddress,
    string? GuestContactNo,
    int? SubTotal,
    double? VATPer,
    double? VATAmount,
    double? STPer,
    double? STAmount,
    int? GrandTotal,
    int? TotalPayment,
    int? PaymentDue,
    IReadOnlyList<OrderLineRow> Lines,
    HotelHeaderRow Hotel);

/// <summary>rptRestaurantOrderReceipt — Restaurant_OrderInfo + Restaurant_Ordered_Product + HotelInfo + CurrencySet.</summary>
public record RestaurantReceiptRow(
    int OrderId,
    string? OrderNo,
    DateTime? OrderDate,
    string? Currency,
    int? SubTotal,
    double? VATPer,
    double? VATAmount,
    double? STPer,
    double? STAmount,
    int? GrandTotal,
    int? TotalPayment,
    int? PaymentDue,
    IReadOnlyList<OrderLineRow> Lines,
    HotelHeaderRow Hotel);

/// <summary>rptSalarySlip — EmployeePayment + EmployeeRegistration.</summary>
public record SalarySlipRow(
    string PaymentID,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? EmployeeID,
    string? EmployeeName,
    string? Designation,
    string? Department,
    int? Salary,
    int? PresentDays,
    int? Advance,
    int? Deduction,
    string? Overtime,
    int? OverTimeAmount,
    DateTime? PaymentDate,
    string? ModeOfPayment,
    int? NetPay,
    HotelHeaderRow Hotel);

/// <summary>rptAttendance — EmployeeAttendance + EmployeeRegistration.</summary>
public record AttendanceRow(
    string? EmployeeID,
    string? EmployeeName,
    DateTime? WorkingDate,
    string? BasicWorkingTime,
    string? Status,
    string? InTime,
    string? OutTime,
    string? Overtime);

/// <summary>rptAdvancePayment — AdvanceEntry + EmployeeRegistration.</summary>
public record AdvancePaymentRow(
    string? EmployeeID,
    string? EmployeeName,
    DateTime? WorkingDate,
    int? Amount,
    int? Deduction);

/// <summary>rptEmployeePayment — EmployeePayment + EmployeeRegistration.</summary>
public record EmployeePaymentRow(
    string PaymentID,
    string? EmployeeID,
    string? EmployeeName,
    DateTime? DateFrom,
    DateTime? DateTo,
    int? PresentDays,
    int? Salary,
    int? Advance,
    int? Deduction,
    string? Overtime,
    int? OverTimeAmount,
    DateTime? PaymentDate,
    string? ModeOfPayment,
    int? NetPay);

/// <summary>rptGuest — Guest.</summary>
public record GuestRow(
    string GuestID,
    string? GuestName,
    string? Address,
    string? City,
    string? ContactNo,
    string? IDType,
    string? IDNumber,
    string? Notes);

/// <summary>rptReservation — Reservation + Guest + Room.</summary>
public record ReservationRow(
    string ReservationID,
    string? GuestID,
    string? GuestName,
    string? RoomNo,
    string? RoomType,
    DateTime? DateIN,
    DateTime? DateOUT,
    string? Status,
    string? Notes);

/// <summary>rptPurchasedInventory — Purchased_Inventory.</summary>
public record PurchasedInventoryRow(
    int ID,
    string? ProductName,
    string? Category,
    string? TransactionType,
    string? PartyName,
    DateTime? PurchaseDate,
    double? Quantity,
    string? Unit,
    int? Price,
    int? TotalPrice);

/// <summary>Lightweight row used by report pages to pick an invoice/slip to render.</summary>
public record InvoicePickRow(string Id, string? Number, string? Name, DateTime? Date);
