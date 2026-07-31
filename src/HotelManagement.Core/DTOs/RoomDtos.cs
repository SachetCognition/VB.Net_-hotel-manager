namespace HotelManagement.Core.DTOs;

public record CreateRoomRequest(string RoomNo, string RoomType, decimal RoomCharges);
public record UpdateRoomRequest(string RoomType, decimal RoomCharges);
public record RoomResponse(string RoomNo, string RoomType, decimal RoomCharges);
