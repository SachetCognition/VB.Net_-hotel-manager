using HotelManager.Application.Billing;
using HotelManager.Domain.Entities;

namespace HotelManager.Application.Interfaces;

// ============================================================
// FROZEN CONTRACTS — child workstreams implement these but must
// not modify them. Request changes from the lead session.
// ============================================================

public record AuthResult(bool Succeeded, string? UserName, string? UserType, string? Error);

public interface IAuthService
{
    Task<AuthResult> ValidateCredentialsAsync(string userName, string password);
    Task<Registration?> GetUserAsync(string userName);
    Task<IReadOnlyList<Registration>> GetUsersAsync();
    Task CreateUserAsync(Registration user, string password);
    Task ChangePasswordAsync(string userName, string newPassword);
    Task DeleteUserAsync(string userName);
    string HashPassword(string password);
}

public interface IHotelInfoService
{
    Task<HotelInfo?> GetAsync();
    Task SaveAsync(HotelInfo info);
}

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencySet>> GetAllAsync();
    Task<CurrencySet?> GetAsync(int id);
    Task<CurrencySet> CreateAsync(CurrencySet currency);
    Task UpdateAsync(CurrencySet currency);
    Task DeleteAsync(int id);
}

public interface IRoomService
{
    Task<IReadOnlyList<Room>> GetRoomsAsync(string? search = null);
    Task<Room?> GetRoomAsync(string roomNo);
    Task CreateRoomAsync(Room room);
    Task UpdateRoomAsync(Room room);
    Task DeleteRoomAsync(string roomNo);
    Task<IReadOnlyList<Hall>> GetHallsAsync();
    Task<Hall> SaveHallAsync(Hall hall);
    Task DeleteHallAsync(int id);
    Task<IReadOnlyList<Garden>> GetGardensAsync();
    Task<Garden> SaveGardenAsync(Garden garden);
    Task DeleteGardenAsync(int id);
    Task<IReadOnlyList<ExtraBed>> GetExtraBedsAsync();
    Task<ExtraBed> SaveExtraBedAsync(ExtraBed bed);
    Task DeleteExtraBedAsync(int id);
}

public interface IGuestService
{
    Task<IReadOnlyList<Guest>> GetAllAsync(string? search = null);
    Task<Guest?> GetAsync(string guestId);
    Task<string> GenerateGuestIdAsync();
    Task CreateAsync(Guest guest);
    Task UpdateAsync(Guest guest);
    Task DeleteAsync(string guestId);
}

public record RoomAvailability(string RoomNo, string? RoomType, int? RoomCharges, bool IsAvailable);

public interface IReservationService
{
    Task<IReadOnlyList<Reservation>> GetReservationsAsync(string? search = null);
    Task<Reservation?> GetReservationAsync(string reservationId);
    Task<string> GenerateReservationIdAsync();
    Task<bool> IsRoomAvailableAsync(string roomNo, DateTime dateIn, DateTime dateOut, string? excludeReservationId = null);
    Task<IReadOnlyList<RoomAvailability>> GetRoomAvailabilityAsync(DateTime dateIn, DateTime dateOut);
    Task CreateReservationAsync(Reservation reservation);
    Task UpdateReservationAsync(Reservation reservation);
    Task CancelReservationAsync(string reservationId);
    Task<IReadOnlyList<TempReservation>> GetTempReservationsAsync();
    Task SaveTempReservationAsync(TempReservation reservation);
    Task DeleteTempReservationAsync(string reservationId);
    Task ConfirmTempReservationAsync(string reservationId);
    Task<IReadOnlyList<ReservationHallAndGarden>> GetHallAndGardenReservationsAsync();
    Task SaveHallAndGardenReservationAsync(ReservationHallAndGarden reservation, TaxReservationHallAndGarden? tax = null);
    Task DeleteHallAndGardenReservationAsync(string id);
    Task<IReadOnlyList<ReservationHallOrGarden>> GetHallOrGardenReservationsAsync();
    Task SaveHallOrGardenReservationAsync(ReservationHallOrGarden reservation, TaxReservationHallOrGarden? tax = null);
    Task DeleteHallOrGardenReservationAsync(string id);
}

public interface ICheckInService
{
    Task<IReadOnlyList<CheckInRoom>> GetCheckInsAsync(string? status = null, string? search = null);
    Task<CheckInRoom?> GetCheckInAsync(int id);
    Task<CheckInRoom> CheckInAsync(CheckInRoom checkIn);
    Task UpdateCheckInAsync(CheckInRoom checkIn);
    Task DeleteCheckInAsync(int id);
}

public interface ICheckOutService
{
    Task<IReadOnlyList<CheckoutRoom>> GetCheckoutsAsync(string? search = null);
    Task<CheckoutRoom?> GetCheckoutAsync(int id);
    Task<string> GenerateBillNoAsync();
    /// <summary>Checks out a checked-in stay. Must prevent double checkout and persist Tax_Room.</summary>
    Task<CheckoutRoom> CheckOutAsync(int checkInId, CheckoutRoom checkout, TaxRoom? tax = null);
}

public interface IRestaurantOrderService
{
    Task<IReadOnlyList<RestaurantOrderInfo>> GetRestaurantOrdersAsync(string? search = null);
    Task<RestaurantOrderInfo?> GetRestaurantOrderAsync(int id);
    Task<string> GenerateRestaurantOrderNoAsync();
    Task<RestaurantOrderInfo> SaveRestaurantOrderAsync(RestaurantOrderInfo order, IEnumerable<RestaurantOrderedProduct> products, TaxRestaurantOrder? tax = null);
    Task DeleteRestaurantOrderAsync(int id);
    Task<IReadOnlyList<OrderInfo>> GetRoomOrdersAsync(string? search = null);
    Task<OrderInfo?> GetRoomOrderAsync(int id);
    Task<string> GenerateRoomOrderNoAsync();
    Task<OrderInfo> SaveRoomOrderAsync(OrderInfo order, IEnumerable<OrderedProduct> products, TaxOrder? tax = null);
    Task DeleteRoomOrderAsync(int id);
}

public interface ITransactionService
{
    Task<IReadOnlyList<Trans>> GetAllAsync(string? search = null);
    Task<Trans?> GetAsync(int id);
    Task<Trans> SaveAsync(Trans transaction);
    Task DeleteAsync(int id);
}

public interface IInventoryService
{
    Task<IReadOnlyList<Dish>> GetDishesAsync(string? search = null);
    Task<Dish> SaveDishAsync(Dish dish);
    Task DeleteDishAsync(int id);
    Task<IReadOnlyList<Beer>> GetBeersAsync(string? search = null);
    Task<Beer> SaveBeerAsync(Beer beer);
    Task DeleteBeerAsync(string id);
    Task<IReadOnlyList<Liquor>> GetLiquorsAsync(string? search = null);
    Task<Liquor> SaveLiquorAsync(Liquor liquor);
    Task DeleteLiquorAsync(int id);
    Task<IReadOnlyList<LiquorMaster>> GetLiquorMastersAsync();
    Task<LiquorMaster> SaveLiquorMasterAsync(LiquorMaster master);
    Task DeleteLiquorMasterAsync(string liquorName);
    Task<IReadOnlyList<Stock>> GetStocksAsync(string? search = null);
    Task<Stock> SaveStockAsync(Stock stock);
    Task DeleteStockAsync(string stockId);
    Task<IReadOnlyList<StockBeer>> GetBeerStocksAsync(string? search = null);
    Task<StockBeer> SaveBeerStockAsync(StockBeer stock);
    Task DeleteBeerStockAsync(string stockId);
    Task<IReadOnlyList<PurchasedInventory>> GetPurchasedInventoriesAsync(string? search = null);
    Task<PurchasedInventory> SavePurchasedInventoryAsync(PurchasedInventory inventory);
    Task DeletePurchasedInventoryAsync(int id);
    Task<IReadOnlyList<TaxInfo>> GetTaxInfosAsync();
    Task<TaxInfo> SaveTaxInfoAsync(TaxInfo taxInfo);
    Task DeleteTaxInfoAsync(string key);
}

public interface IHrPayrollService
{
    Task<IReadOnlyList<EmployeeRegistration>> GetEmployeesAsync(string? search = null);
    Task<EmployeeRegistration?> GetEmployeeAsync(string employeeId);
    Task<string> GenerateEmployeeIdAsync();
    Task SaveEmployeeAsync(EmployeeRegistration employee, bool isNew);
    Task DeleteEmployeeAsync(string employeeId);
    Task<IReadOnlyList<EmployeeAttendance>> GetAttendanceAsync(string? employeeId = null, DateTime? from = null, DateTime? to = null);
    Task<EmployeeAttendance> SaveAttendanceAsync(EmployeeAttendance attendance);
    Task DeleteAttendanceAsync(int attendanceId);
    Task<IReadOnlyList<AdvanceEntry>> GetAdvanceEntriesAsync(string? employeeId = null);
    Task<AdvanceEntry> SaveAdvanceEntryAsync(AdvanceEntry entry);
    Task DeleteAdvanceEntryAsync(int id);
    Task<int> GetOutstandingAdvanceAsync(string employeeId, DateTime from, DateTime to);
    Task<TimeSpan> GetTotalOvertimeAsync(string employeeId, DateTime from, DateTime to);
    Task<int> GetPresentDaysAsync(string employeeId, DateTime from, DateTime to);
    Task<IReadOnlyList<EmployeePayment>> GetPaymentsAsync(string? employeeId = null);
    Task<string> GeneratePaymentIdAsync();
    /// <summary>Runs payroll with BillingCalculator guards; must reject if already paid today.</summary>
    Task<EmployeePayment> RunPaymentAsync(EmployeePayment payment);
    Task DeletePaymentAsync(string paymentId);
}

public record ReportRequest(DateTime? From = null, DateTime? To = null, string? Key = null);

public interface IReportService
{
    Task<byte[]> RenderRoomInvoicePdfAsync(int checkoutId);
    Task<byte[]> RenderHallAndGardenInvoicePdfAsync(string reservationId);
    Task<byte[]> RenderHallOrGardenInvoicePdfAsync(string reservationId);
    Task<byte[]> RenderOrderInvoicePdfAsync(int orderId);
    Task<byte[]> RenderRestaurantOrderReceiptPdfAsync(int orderId);
    Task<byte[]> RenderSalarySlipPdfAsync(string paymentId);
    Task<byte[]> RenderAttendanceReportPdfAsync(ReportRequest request);
    Task<byte[]> RenderAdvancePaymentReportPdfAsync(ReportRequest request);
    Task<byte[]> RenderEmployeePaymentReportPdfAsync(ReportRequest request);
    Task<byte[]> RenderGuestReportPdfAsync(ReportRequest request);
    Task<byte[]> RenderReservationReportPdfAsync(ReportRequest request);
    Task<byte[]> RenderPurchasedInventoryReportPdfAsync(ReportRequest request);
}
