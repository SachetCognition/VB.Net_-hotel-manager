using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface ICheckInService
{
    Task<CheckInResponse> CheckInAsync(CreateCheckInRequest request);
    Task<CheckInResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CheckInResponse>> GetAllAsync(string? roomNo = null, string? guestName = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task DeleteAsync(int id);
    TaxCalculationResult CalculateTaxes(decimal roomCharges, int noOfDays, decimal otherCharges, decimal discountPer, decimal serviceTaxPer, decimal luxuryTaxPer, decimal totalPaid);
    int CalculateNoOfDays(DateTime dateIn, DateTime dateOut);
    Task<bool> IsRoomAvailableAsync(string roomNo, DateTime dateIn, DateTime dateOut, int? excludeId = null);
}
