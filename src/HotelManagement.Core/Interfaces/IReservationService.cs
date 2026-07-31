using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request);
    Task<ReservationResponse?> GetByIdAsync(int id);
    Task<IEnumerable<ReservationResponse>> GetAllAsync(string? roomNo = null, string? guestName = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ReservationResponse> UpdateAsync(int id, CreateReservationRequest request);
    Task DeleteAsync(int id);
    Task<bool> CheckDateOverlapAsync(string roomNo, DateTime dateIn, DateTime dateOut, int? excludeId = null);
}
