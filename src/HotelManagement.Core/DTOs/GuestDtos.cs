namespace HotelManagement.Core.DTOs;

public record CreateGuestRequest(
    string GuestName, string Address, string City,
    string ContactNo, string IDType, string IDNumber, string Notes);

public record UpdateGuestRequest(
    string GuestName, string Address, string City,
    string ContactNo, string IDType, string IDNumber, string Notes);

public record GuestResponse(
    string GuestID, string GuestName, string Address, string City,
    string ContactNo, string IDType, string IDNumber, string Notes);
