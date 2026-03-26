using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IRoomService
{
    Task<RoomResponse> CreateAsync(CreateRoomRequest request);
    Task<RoomResponse?> GetByIdAsync(string roomNo);
    Task<IEnumerable<RoomResponse>> GetAllAsync(string? search = null);
    Task<RoomResponse> UpdateAsync(string roomNo, UpdateRoomRequest request);
    Task DeleteAsync(string roomNo);
    Task<IEnumerable<RoomResponse>> GetAvailableRoomsAsync(DateTime dateIn, DateTime dateOut);
}
