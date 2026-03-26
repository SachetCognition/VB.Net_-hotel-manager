using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IGuestService
{
    Task<GuestResponse> CreateAsync(CreateGuestRequest request);
    Task<GuestResponse?> GetByIdAsync(string guestId);
    Task<IEnumerable<GuestResponse>> GetAllAsync(string? search = null);
    Task<GuestResponse> UpdateAsync(string guestId, UpdateGuestRequest request);
    Task DeleteAsync(string guestId);
    string GenerateGuestId();
}
