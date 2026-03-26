using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface ICheckOutService
{
    Task<CheckOutResponse> CheckOutAsync(CheckOutRequest request);
    Task<CheckOutResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CheckOutResponse>> GetAllAsync(string? roomNo = null, string? guestName = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task DeleteAsync(int id);
    CheckOutTaxResult CalculateEducationCess(decimal serviceTaxPer, decimal serviceTaxAmount);
}
