namespace HotelManagement.Core.DTOs;

public record CreateReservationRequest(
    string GuestID, string GuestName, string RoomNo, string RoomType,
    decimal RoomCharges, DateTime DateIN, DateTime DateOUT,
    int NoOfAdults, int NoOfKids, string Currency, string Notes);

public record ReservationResponse(
    int ID, string GuestID, string GuestName, string RoomNo, string RoomType,
    decimal RoomCharges, DateTime DateIN, DateTime DateOUT,
    int NoOfAdults, int NoOfKids, string Status, string Currency, string Notes);

public record CreateHallGardenReservationRequest(
    string GuestID, string GuestName, string HallName, string GardenName,
    DateTime DateIN, DateTime DateOUT, int NoOfDaysHall, int NoOfDaysGarden,
    decimal RateHall, decimal RateGarden, decimal OtherCharges,
    decimal DiscountPer, decimal ServiceTaxPer, decimal LuxuryTaxPer,
    decimal TotalPaid, string Currency, string Notes);

public record CreateHallOrGardenReservationRequest(
    string GuestID, string GuestName, string VenueName, string VenueType,
    DateTime DateIN, DateTime DateOUT, int NoOfDays, decimal Rate,
    decimal OtherCharges, decimal DiscountPer, decimal ServiceTaxPer,
    decimal LuxuryTaxPer, decimal TotalPaid, string Currency, string Notes);
